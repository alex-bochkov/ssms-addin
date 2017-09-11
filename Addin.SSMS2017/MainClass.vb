Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel.Design
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms
Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.SqlServer.Management.UI.VSIntegration
Imports Microsoft.SqlServer.TransactSql.ScriptDom
Imports Microsoft.VisualStudio.CommandBars
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.Shell.Interop
Imports OfficeOpenXml.Style

''' <summary>
''' Command handler
''' </summary>
Public NotInheritable Class MainClass

    ''' <summary>
    ''' Command ID.
    ''' </summary>
    Public Const CommandIdFormatTSQL As Integer = 254
    Public Const CommandIdExportToExcel As Integer = 255
    Public Const CommandIdSettingForm As Integer = 200
    Public Const cmdidInheritanceTargetsList As Integer = 450

    Private TemplatesList As Hashtable = New Hashtable
    Dim UpperBoundary As Integer = 0

    ''' <summary>
    ''' Command menu group (command set GUID).
    ''' </summary>
    Public Shared ReadOnly CommandSet As New Guid("23791ae2-2f65-4799-b136-1540ce646ebc")

    ''' <summary>
    ''' VS Package that provides this command, not null.
    ''' </summary>
    Private ReadOnly package As Package


    ''' <summary>
    ''' Initializes a new instance of the <see cref="ExportToExcel"/> class.
    ''' Adds our command handlers for menu (the commands must exist in the command table file)
    ''' </summary>
    ''' <param name="package">Owner package, not null.</param>
    Private Sub New(package As Package)
        If package Is Nothing Then
            Throw New ArgumentNullException("package")
        End If

        Me.package = package
        Dim commandService As OleMenuCommandService = Me.ServiceProvider.GetService(GetType(IMenuCommandService))
        If commandService IsNot Nothing Then

            Dim menuCommandIdExportToExcel = New CommandID(CommandSet, CommandIdExportToExcel)
            Dim menuCommandExportToExcel = New MenuCommand(AddressOf Me.MenuItemCallback_ExportToExcel, menuCommandIdExportToExcel)
            commandService.AddCommand(menuCommandExportToExcel)

            Dim menuCommandIdFormatTSQL = New CommandID(CommandSet, CommandIdFormatTSQL)
            Dim menuCommandFormatTSQL = New MenuCommand(AddressOf Me.MenuItemCallback_FormatTSQL, menuCommandIdFormatTSQL)
            commandService.AddCommand(menuCommandFormatTSQL)

            Dim menuCommandIdSettingForm = New CommandID(CommandSet, CommandIdSettingForm)
            Dim menuCommandSettingForm = New MenuCommand(AddressOf Me.MenuItemCallback_SettingForm, menuCommandIdSettingForm)
            commandService.AddCommand(menuCommandSettingForm)



            Dim dynamicItemRootId As New CommandID(CommandSet, cmdidInheritanceTargetsList)
            Dim dynamicMenuCommand As New DynamicItemMenuCommand(dynamicItemRootId, AddressOf IsValidDynamicItem, AddressOf OnInvokedDynamicItem, AddressOf OnBeforeQueryStatusDynamicItem)
            commandService.AddCommand(dynamicMenuCommand)


        End If

        RecreateTemplates()


    End Sub



    ''' <summary>
    ''' Gets the instance of the command.
    ''' </summary>
    Public Shared Property Instance As MainClass

    ''' <summary>
    ''' Get service provider from the owner package.
    ''' </summary>
    Private ReadOnly Property ServiceProvider As IServiceProvider
        Get
            Return Me.package
        End Get
    End Property

    ''' <summary>
    ''' Initializes the singleton instance of the command.
    ''' </summary>
    ''' <param name="package">Owner package, Not null.</param>
    Public Shared Sub Initialize(package As Package)
        Instance = New MainClass(package)


    End Sub

    Private Sub MenuItemCallback_SettingForm(sender As Object, e As EventArgs)
        Dim message = String.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", Me.GetType().FullName)
        Dim title = "SettingForm"


        Dim SettingUI = New SettingFormUI
        SettingUI.ShowDialog()


        '' Show a message box to prove we were here
        'VsShellUtilities.ShowMessageBox(
        '    Me.ServiceProvider,
        '    message,
        '    title,
        '    OLEMSGICON.OLEMSGICON_INFO,
        '    OLEMSGBUTTON.OLEMSGBUTTON_OK,
        '    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)
    End Sub

    Private Sub MenuItemCallback_FormatTSQL(sender As Object, e As EventArgs)

        Dim title = "Format TSQL"


        Try

            Dim document As Document = (DirectCast(ServiceCache.ExtensibilityModel, DTE2)).ActiveDocument


            Dim txt As TextDocument = CType(document.Object("TextDocument"), TextDocument)

            'get an edit point
            Dim ep As EditPoint = txt.Selection.ActivePoint.CreateEditPoint

            'clear the selection
            Dim OldStr = txt.Selection.Text


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

            If Not txt.Selection.IsEmpty Then
                txt.Selection.Delete()
            End If

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

            ep.Insert(StrAdd2)


        Catch ex As Exception
            VsShellUtilities.ShowMessageBox(
            Me.ServiceProvider,
            ex.Message,
            title,
            OLEMSGICON.OLEMSGICON_CRITICAL,
            OLEMSGBUTTON.OLEMSGBUTTON_OK,
            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)
        End Try


    End Sub

    ''' <summary>
    ''' This function is the callback used to execute the command when the menu item is clicked.
    ''' See the constructor to see how the menu item is associated with this function using
    ''' OleMenuCommandService service and MenuCommand class.
    ''' </summary>
    ''' <param name="sender">Event sender.</param>
    ''' <param name="e">Event args.</param>
    Private Sub MenuItemCallback_ExportToExcel(sender As Object, e As EventArgs)
        'Dim message = String.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", Me.GetType().FullName)
        Dim title = "Export to MS Excel"

        '' Show a message box to prove we were here
        'VsShellUtilities.ShowMessageBox(
        '    Me.ServiceProvider,
        '    message,
        '    title,
        '    OLEMSGICON.OLEMSGICON_INFO,
        '    OLEMSGBUTTON.OLEMSGBUTTON_OK,
        '    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)


        Dim document As Document = (DirectCast(ServiceCache.ExtensibilityModel, DTE2)).ActiveDocument


        'Dim sqlScriptEditorControl As Object = InvokeMethod(ServiceCache.ScriptFactory, "GetCurrentlyActiveFrameDocView", BindingFlags.NonPublic Or BindingFlags.Instance, New Object() {ServiceCache.VSMonitorSelection, False, Nothing})
        ' m_SQLResultsControl = GetField(sqlScriptEditorControl, "m_sqlResultsControl", BindingFlags.NonPublic Or BindingFlags.Instance)

        Dim objType = ServiceCache.ScriptFactory.GetType()
        Dim method1 = objType.GetMethod("GetCurrentlyActiveFrameDocView", BindingFlags.NonPublic Or BindingFlags.Instance)
        Dim Result = method1.Invoke(ServiceCache.ScriptFactory, New Object() {ServiceCache.VSMonitorSelection, False, Nothing})

        Dim objType2 = Result.GetType()
        Dim field = objType2.GetField("m_sqlResultsControl", BindingFlags.NonPublic Or BindingFlags.Instance)
        Dim SQLResultsControl = field.GetValue(Result)

        Dim m_gridResultsPage = GetNonPublicField(SQLResultsControl, "m_gridResultsPage")
        Dim gridContainers As CollectionBase = GetNonPublicField(m_gridResultsPage, "m_gridContainers")


        Dim Folder As String = SettingManager.GetExcelExportFolder()
        Dim FileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm")
        Dim FullFilename = Path.Combine(Folder, "QueryResult_" + FileName + ".xlsx")
        If My.Computer.FileSystem.FileExists(FullFilename) Then
            FullFilename = Path.Combine(Folder, "QueryResult_" + FileName + "_" + Guid.NewGuid.ToString + ".xlsx")
        End If

        Dim i = 0

        For Each GridContainer In gridContainers

            i = i + 1

            Dim Grid = GetNonPublicField(GridContainer, "m_grid")
            Dim GridStorage = Grid.GridStorage
            Dim SchemaTable = GetNonPublicField(GridStorage, "m_schemaTable")

            ExportDataSet(FullFilename, GridStorage, SchemaTable, i)

        Next

        Dim d As New DataObject(DataFormats.Text, FullFilename)
        Clipboard.SetDataObject(d, True)

        Dim msgText = "File saved to: " + FullFilename + Environment.NewLine + "Filename copied to clipboard."

        ' Show a message box to prove we were here
        VsShellUtilities.ShowMessageBox(
        Me.ServiceProvider,
        msgText,
        title,
        OLEMSGICON.OLEMSGICON_INFO,
        OLEMSGBUTTON.OLEMSGBUTTON_OK,
        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)

    End Sub

    Function GetNonPublicField(obj As Object, field As String) As Object

        Dim f As FieldInfo = obj.GetType().GetField(field, BindingFlags.NonPublic Or BindingFlags.Instance)
        Return f.GetValue(obj)

    End Function

    Private Function ExportDataSet(FullFilename As String, ds As Object, SchemaTable As Object, QueryNumber As Integer) As String

        Dim newFile As New FileInfo(FullFilename)
        'If newFile.Exists Then
        'newFile.Open(FileMode.Append, FileAccess.Write)
        '    newFile = New FileInfo(FullFilename)
        'End If

        Using package As New OfficeOpenXml.ExcelPackage(newFile)
            Dim worksheet As OfficeOpenXml.ExcelWorksheet = package.Workbook.Worksheets.Add("Query_" + QueryNumber.ToString)

            Dim ColumnTypes = New Hashtable
            Dim kk = 0

            For Each Column In SchemaTable.Rows

                kk = kk + 1

                Dim TypeStr = Column(12).ToString
                Dim ColumnName = ""
                If String.IsNullOrEmpty(Column(0).ToString) Then
                    ColumnName = "(no column name)"
                Else
                    ColumnName = Column(0).ToString
                End If

                ColumnTypes.Add(kk, TypeStr)

                worksheet.Cells(1, kk).Value = ColumnName

            Next

            Using range = worksheet.Cells(1, 1, 1, kk)
                range.Style.Font.Bold = True
                range.Style.Fill.PatternType = ExcelFillStyle.Solid
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue)
                range.Style.Font.Color.SetColor(System.Drawing.Color.White)
            End Using


            For iRow = 0 To ds.TotalNumberOfRows - 1
                For iCol = 1 To ds.TotalNumberOfColumns - 1

                    Dim Val = ds.GetCellData(iRow, iCol)

                    If Val Is Nothing Then
                        worksheet.Cells(iRow + 2, iCol).Value = ds.GetCellDataAsString(iRow, iCol)
                    Else

                        If TypeOf (Val) Is String Then
                            worksheet.Cells(iRow + 2, iCol).Value = Val
                        Else
                            worksheet.Cells(iRow + 2, iCol).Value = Val.Value
                            If TypeOf (Val.value) Is Date Then
                                worksheet.Cells(iRow + 2, iCol).Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss"
                            End If
                        End If


                    End If

                Next
            Next

            worksheet.Cells.AutoFitColumns(0)

            package.Save()

        End Using


        Return True

    End Function

    Structure ScriptInfo
        Dim ShortName As String
        Dim FullName As String

    End Structure

    Private Sub RecreateTemplates()

        TemplatesList = New Hashtable
        Dim Folder = SettingManager.GetTemplatesFolder()

        Dim Files = My.Computer.FileSystem.GetFiles(Folder, Microsoft.VisualBasic.FileIO.SearchOption.SearchAllSubDirectories)
        Dim i = 600 - 1

        For Each File In Files

            i = i + 1

            Dim FI = My.Computer.FileSystem.GetFileInfo(File)

            TemplatesList.Add(i, New ScriptInfo() With {.ShortName = FI.Name, .FullName = FI.FullName})

        Next

        UpperBoundary = i

        'Dim a = EnvDTE.AddIns

        'Dim document As Document = (DirectCast(ServiceCache.ExtensibilityModel, DTE2))
        'Dim CommandBars = DirectCast(ServiceCache.ExtensibilityModel, CommandBar)
        Dim CommandBars = ServiceCache.ExtensibilityModel.CommandBars
        Dim myTemporaryToolbar = CommandBars.Item("SSMS2017.Addin") 'CommandBars.Add("SSMSAddin", MsoBarPosition.msoBarTop, System.Type.Missing, True)

        For Each Cmd2 In DirectCast(ServiceCache.ExtensibilityModel.Commands, Commands2)
            If Cmd2.Name.ToString.Contains("SSMSTemplates") Then
                Cmd2.Delete()
            End If
        Next

        Dim ErrorText = ""

        Try

            'Method invocation failed because 'Public Overrides Function Add(Type As System.Object, Id As System.Object, Parameter As System.Object, Before As System.Object, Temporary As System.Object) As Microsoft.VisualStudio.CommandBars.CommandBarControl' cannot be called with these arguments:
            'Argument Not specified for parameter 'Id'.
            'Argument Not specified for parameter 'Parameter'.
            'Argument Not specified for parameter 'Before'.

            Dim myTemporaryPopup = myTemporaryToolbar.Controls.Add(MsoControlType.msoControlPopup, Nothing, Nothing, 4, True)
            myTemporaryPopup.Caption = "Templates"
            myTemporaryPopup.BeginGroup = False
            myTemporaryPopup.CommandBar.Name = "Templates"
            myTemporaryPopup.Visible = True

            Dim Btn As CommandBarButton = myTemporaryPopup.Controls.add(MsoControlType.msoControlButton, Nothing, Nothing, 1, True)
            Btn.Caption = "Test button"
            Btn.Visible = True
            'Btn.
            'Btn.Click = AddressOf ButtonCommandTemplate_Click

            If Not String.IsNullOrEmpty(Folder) Then
                Try
                    CreateCommandsInRecursion(myTemporaryPopup.CommandBar, Folder, i)
                Catch ex As Exception
                    'ErrorText = ex.Message

                    'VsShellUtilities.ShowMessageBox(
                    '   Me.ServiceProvider,
                    '   ErrorText,
                    '   "ERROR",
                    '   OLEMSGICON.OLEMSGICON_CRITICAL,
                    '   OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    '   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)
                End Try

            End If
        Catch ex As Exception
            ErrorText = ex.Message

            VsShellUtilities.ShowMessageBox(
               Me.ServiceProvider,
               ErrorText,
               "ERROR",
               OLEMSGICON.OLEMSGICON_CRITICAL,
               OLEMSGBUTTON.OLEMSGBUTTON_OK,
               OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)
        End Try



    End Sub

    Private Sub ButtonCommandTemplate_Click(sender As Object, args As EventArgs)
        Dim a = 0
    End Sub

    Sub CreateCommandsInRecursion(ByRef Owner As CommandBar, Folder As String, ByRef i As Integer)

        Dim Dirs = My.Computer.FileSystem.GetDirectories(Folder)

        For Each DirStr In Dirs

            Dim DI = My.Computer.FileSystem.GetFileInfo(DirStr)

            Dim popup2 As CommandBarPopup = Owner.Controls.Add(MsoControlType.msoControlPopup, Nothing, Nothing, 1, True)

            popup2.Caption = DI.Name
            popup2.BeginGroup = False
            'popup2.CommandBar.Name = "Templates"
            popup2.Visible = True

            CreateCommandsInRecursion(popup2.CommandBar, DirStr, i)



            i = i + 1
        Next


        Dim Files = My.Computer.FileSystem.GetFiles(Folder)

        For Each File In Files

            Dim FI = My.Computer.FileSystem.GetFileInfo(File)

            Dim cmd As Command = ServiceCache.ExtensibilityModel.Commands.AddNamedCommand(Nothing, "SSMSTemplates_" + i.ToString, FI.Name, "", True, 2687,
                                                                 Nothing, vsCommandStatus.vsCommandStatusSupported Or vsCommandStatus.vsCommandStatusEnabled)

            Dim myToolBarButton = DirectCast(cmd.AddControl(Owner, Owner.Controls.Count + 1), CommandBarButton)

            myToolBarButton.Caption = FI.Name
            ' myToolBarButton.Parameter = FI.FullName
            'myToolBarButton.BeginGroup = Owner
            myToolBarButton.Style = MsoButtonStyle.msoButtonIconAndCaption ' It could be also msoButtonIcon

            TemplatesList.Add(cmd.Name, FI.FullName)

            i = i + 1


        Next


    End Sub



    '***************************************************************************
    Private Class DynamicItemMenuCommand
        Inherits OleMenuCommand

        Private matches As Predicate(Of Integer)

        Public Sub New(rootId As CommandID, matches As Predicate(Of Integer), invokeHandler As EventHandler, beforeQueryStatusHandler As EventHandler)
            'changeHandler
            MyBase.New(invokeHandler, Nothing, beforeQueryStatusHandler, rootId)
            If matches Is Nothing Then
                Throw New ArgumentNullException("matches")
            End If

            Me.matches = matches
        End Sub

        Public Overrides Function DynamicItemMatch(cmdId As Integer) As Boolean
            ' Call the supplied predicate to test whether the given cmdId is a match.  
            ' If it is, store the command id in MatchedCommandid   
            ' for use by any BeforeQueryStatus handlers, and then return that it is a match.  
            ' Otherwise clear any previously stored matched cmdId and return that it is not a match.  
            If Me.matches(cmdId) Then
                Me.MatchedCommandId = cmdId
                Return True
            End If

            Me.MatchedCommandId = 0
            Return False
        End Function

    End Class

    Private Sub OnInvokedDynamicItem(sender As Object, args As EventArgs)

        Dim invokedCommand As DynamicItemMenuCommand = DirectCast(sender, DynamicItemMenuCommand)



        If TemplatesList.ContainsKey(invokedCommand.MatchedCommandId) Then
            Dim TemplateFile As ScriptInfo = TemplatesList.Item(invokedCommand.MatchedCommandId)
            If Not TemplateFile.FullName Is Nothing Then

                Dim document As Document = (DirectCast(ServiceCache.ExtensibilityModel, DTE2)).ActiveDocument

                Dim Text = My.Computer.FileSystem.ReadAllText(TemplateFile.FullName)

                Dim txt As TextDocument = CType(document.Object("TextDocument"), TextDocument)

                'get an edit point
                Dim ep As EditPoint = txt.Selection.ActivePoint.CreateEditPoint

                'clear the selection
                If Not txt.Selection.IsEmpty Then txt.Selection.Delete()

                ep.Insert(Text)

            End If
        End If



        '' Show a message box to prove we were here
        'VsShellUtilities.ShowMessageBox(
        '    Me.ServiceProvider,
        '    invokedCommand.Text,
        '    "Dynamic buttons",
        '    OLEMSGICON.OLEMSGICON_INFO,
        '    OLEMSGBUTTON.OLEMSGBUTTON_OK,
        '    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)



    End Sub



    Private Sub OnBeforeQueryStatusDynamicItem(sender As Object, args As EventArgs)
        Dim matchedCommand As DynamicItemMenuCommand = DirectCast(sender, DynamicItemMenuCommand)

        Dim isRootItem As Boolean = (matchedCommand.MatchedCommandId = 0)

        matchedCommand.Enabled = True
        matchedCommand.Visible = Not isRootItem

        ' Find out whether the command ID is 0, which is the ID of the root item.  
        ' If it is the root item, it matches the constructed DynamicItemMenuCommand,  
        ' and IsValidDynamicItem won't be called.  


        ' The index is set to 1 rather than 0 because the Solution.Projects collection is 1-based.  
        'Dim indexForDisplay As Integer = (If(isRootItem, 1, (matchedCommand.MatchedCommandId - CInt(cmdidMRUList2)) + 1))

        'TODO
        'matchedCommand.Text = DTE2.Solution.Projects.Item(indexForDisplay).Name

        If TemplatesList.ContainsKey(matchedCommand.MatchedCommandId) Then

            Dim ScriptObj As ScriptInfo = TemplatesList.Item(matchedCommand.MatchedCommandId)

            matchedCommand.Text = ScriptObj.ShortName

        Else
            matchedCommand.Text = "< error fetching the script file >"
        End If


        ''''*********************************
        '''If matchedCommand.MatchedCommandId = 601 Then
        '''    Dim commandService As OleMenuCommandService = Me.ServiceProvider.GetService(GetType(IMenuCommandService))
        '''    Dim dynamicItemRootId As New CommandID(CommandSet, 601)
        '''    Dim dynamicMenuCommand As New DynamicItemMenuCommand(dynamicItemRootId, AddressOf IsValidDynamicItem, AddressOf OnInvokedDynamicItem, AddressOf OnBeforeQueryStatusDynamicItem)
        '''    commandService.AddCommand(dynamicMenuCommand)
        '''End If

        ''''*********************************

        ' Check the command if it isn't checked already selected  
        'matchedCommand.Checked = isRootItem 'True '(matchedCommand.Text = startupProject)

        ' Clear the ID because we are done with this item.  
        'matchedCommand.MatchedCommandId = 0
    End Sub


    Private Function IsValidDynamicItem(commandId As Integer) As Boolean
        ' The match is valid if the command ID is >= the id of our root dynamic start item   
        ' and the command ID minus the ID of our root dynamic start item  
        ' is less than or equal to the number of projects in the solution.  
        Return (commandId >= 600) And (commandId <= UpperBoundary)
        'Return (commandId >= CInt(cmdidMRUList2)) AndAlso ((commandId - CInt(cmdidMRUList2)) < dte2.Solution.Projects.Count)
    End Function



End Class
