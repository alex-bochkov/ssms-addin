Imports System
Imports Extensibility
Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.VisualStudio.CommandBars
Imports Microsoft.SqlServer.Management.UI.VSIntegration
'Imports Microsoft.SqlServer.Management.UI.Grid
Imports System.Resources
Imports System.Reflection
Imports System.Management
Imports System.Globalization
Imports Microsoft.SqlServer.Management
Imports DocumentFormat.OpenXml.Packaging
Imports System.IO
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Spreadsheet
Imports System.Windows.Forms
Imports OfficeOpenXml
Imports OfficeOpenXml.Style
Imports Microsoft.SqlServer.TransactSql.ScriptDom

Public Class Connect

    Implements IDTExtensibility2
    Implements IDTCommandTarget

    Private _applicationObject As DTE2
    Private _addInInstance As AddIn
    Dim myTemporaryToolbar As CommandBar
    Dim myTemporaryPopup As CommandBarPopup

    Private toolWindow As Window

    Private QueryExecuteEvent As CommandEvents

    Private TemplatesList As Hashtable = New Hashtable

    Public Sub New()
        'MsgBox("Test message")
    End Sub

    'Public Sub OnBeforeQueryExecuted(guid As String, id As Integer, customIn As Object, customOut As Object, ByRef cancelDefault As Boolean)

    '    Dim aa = 1

    'End Sub

    'Public Sub OnAfterQueryExecuted(guid As String, id As Integer, customIn As Object, customOut As Object)

    '    Dim aa = 1

    'End Sub

    Public Sub OnConnection(application As Object, connectMode As ext_ConnectMode, addInInst As Object, ByRef [custom] As Array) Implements IDTExtensibility2.OnConnection

        _applicationObject = DirectCast(application, DTE2)
        _addInInstance = DirectCast(addInInst, AddIn)

        'QueryExecuteEvent = _applicationObject.DTE.Events.CommandEvents("{52692960-56BC-4989-B5D3-94C47A513E8D}", 1)
        ' AddHandler QueryExecuteEvent.BeforeExecute, AddressOf OnBeforeQueryExecuted
        'AddHandler QueryExecuteEvent.AfterExecute, AddressOf OnAfterQueryExecuted

        If connectMode = ext_ConnectMode.ext_cm_UISetup Then
            'Dim contextGUIDS As Object() = New Object() {}
            'Dim commands As Commands2 = DirectCast(_applicationObject.Commands, Commands2)
            'Dim toolsMenuName As String = "Tools"
            'Dim menuBarCommandBar As CommandBar = (DirectCast(_applicationObject.CommandBars, Microsoft.VisualStudio.CommandBars.CommandBars))("MenuBar")
            'Dim toolsControl As CommandBarControl = menuBarCommandBar.Controls(toolsMenuName)
            'Dim toolsPopup As CommandBarPopup = DirectCast(toolsControl, CommandBarPopup)
            'Try

            '    For Each Cmd As Command In commands
            '        If Cmd.Name.Contains("SSMSAddin") Then
            '            Cmd.Delete()
            '        End If
            '    Next

            '    Dim command As Command = commands.AddNamedCommand2(_addInInstance, "SSMSAddin", "SSMSAddin", "Executes the command for SSMSAddin", True, 59,
            '        contextGUIDS, DirectCast(vsCommandStatus.vsCommandStatusSupported, Integer) + DirectCast(vsCommandStatus.vsCommandStatusEnabled, Integer), DirectCast(vsCommandStyle.vsCommandStylePictAndText, Integer), vsCommandControlType.vsCommandControlTypeButton)
            '    If (Not command Is Nothing) AndAlso (Not toolsPopup Is Nothing) Then
            '        command.AddControl(toolsPopup.CommandBar, 1)
            '    End If
            'Catch generatedExceptionName As System.ArgumentException
            '    MsgBox(generatedExceptionName.Message)
            'End Try

        ElseIf connectMode = ext_ConnectMode.ext_cm_Startup Then

            RecreateTemplates()

        End If

    End Sub
    Public Sub OnDisconnection(disconnectMode As ext_DisconnectMode, ByRef [custom] As Array) Implements IDTExtensibility2.OnDisconnection

        Try
            '' When the add-in closes, get rid of the toolbar button.
            'If Not (myTemporaryToolbar Is Nothing) Then
            '    myTemporaryToolbar.Delete()
            'End If
            ' When the add-in closes, get rid of the toolbar button.
            If Not (myTemporaryPopup Is Nothing) Then
                myTemporaryPopup.Delete()
            End If


        Catch e As System.Exception
            System.Windows.Forms.MessageBox.Show(e.ToString)
        End Try

    End Sub
    Public Sub OnAddInsUpdate(ByRef [custom] As Array) Implements IDTExtensibility2.OnAddInsUpdate
    End Sub
    Public Sub OnStartupComplete(ByRef [custom] As Array) Implements IDTExtensibility2.OnStartupComplete
    End Sub
    Public Sub OnBeginShutdown(ByRef [custom] As Array) Implements IDTExtensibility2.OnBeginShutdown
    End Sub
    Public Sub QueryStatus(commandName As String, neededText As vsCommandStatusTextWanted, ByRef status As vsCommandStatus, ByRef commandText As Object) Implements IDTCommandTarget.QueryStatus

        If neededText = vsCommandStatusTextWanted.vsCommandStatusTextWantedNone Then
            If commandName = "SSMSTool.Connect.SSMSAddin" Then
                status = vsCommandStatus.vsCommandStatusSupported Or vsCommandStatus.vsCommandStatusEnabled
                Return
            ElseIf commandName.Contains("SSMSTool") Then 'let them all work
                status = vsCommandStatus.vsCommandStatusSupported Or vsCommandStatus.vsCommandStatusEnabled
                Return
            End If
        End If

    End Sub
    Public Sub Exec(commandName As String, executeOption As vsCommandExecOption, ByRef varIn As Object, ByRef varOut As Object, ByRef handled As Boolean) Implements IDTCommandTarget.Exec

        handled = False
        If executeOption = vsCommandExecOption.vsCommandExecOptionDoDefault Then

            If commandName.Contains("SSMSRefreshTemplates") Then

                RecreateTemplates()

            ElseIf commandName.Contains("SSMSSettingForm") Then

                Dim guidString As String = "{9FFC9D9B-1F39-4763-A2AF-66AED06C711E}"
                Dim windows2 As Windows2 = DirectCast(_applicationObject.Windows, Windows2)
                Dim asm As Reflection.Assembly = Assembly.GetExecutingAssembly()
                toolWindow = windows2.CreateToolWindow2(_addInInstance, asm.Location, "SSMSTool.SettingForm", "Addin setting", guidString, Nothing)
                toolWindow.Visible = True

            ElseIf commandName.Contains("SSMSTemplates") Then

                Dim document As Document = (DirectCast(ServiceCache.ExtensibilityModel, DTE2)).ActiveDocument

                Dim TemplateFile = TemplatesList.Item(commandName)
                If Not TemplateFile Is Nothing Then

                    Dim Text = My.Computer.FileSystem.ReadAllText(TemplateFile)

                    Dim txt As TextDocument = CType(document.Object("TextDocument"), TextDocument)

                    'get an edit point
                    Dim ep As EditPoint = txt.Selection.ActivePoint.CreateEditPoint

                    'clear the selection
                    If Not txt.Selection.IsEmpty Then txt.Selection.Delete()

                    ep.Insert(Text)

                End If



            ElseIf commandName.Contains("SSMSExportToExcel") Then

                SaveCurrentGridToExcel()

                handled = True
                Return

            ElseIf commandName.Contains("SSMSFormatSelection") Then

                FormatSelection()

                handled = True
                Return

            End If
        End If

    End Sub

    Sub FormatSelection()

        Try

            Dim document As Document = (DirectCast(ServiceCache.ExtensibilityModel, DTE2)).ActiveDocument


            Dim txt As TextDocument = CType(document.Object("TextDocument"), TextDocument)

            'get an edit point
            Dim ep As EditPoint = txt.Selection.ActivePoint.CreateEditPoint

            'clear the selection
            Dim OldStr = txt.Selection.Text

            Dim SqlParser = New TSql90Parser(False)

            Dim parseErrors As IList(Of ParseError) = New List(Of ParseError)
            Dim result As TSqlFragment = SqlParser.Parse(New StringReader(OldStr), parseErrors)

            If parseErrors.Count > 0 Then
                Throw New System.Exception("TSql90Parser unable format selected T-SQL due to an error in syntax..")
            End If

            If Not txt.Selection.IsEmpty Then
                txt.Selection.Delete()
            End If

            Dim StrAdd2 = ""
            Dim Gen = New Sql90ScriptGenerator
            Gen.Options.IncludeSemicolons = False
            Gen.Options.AlignClauseBodies = False
            Gen.GenerateScript(result, StrAdd2)

            ep.Insert(StrAdd2)


        Catch ex As Exception
            MsgBox(ex.Message)
        End Try







    End Sub

    Sub SaveCurrentGridToExcel()

        Try

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
            Dim FileName = Now.ToString("yyyy-MM-dd_HH-mm")
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

            MsgBox("File saved to: " + FullFilename + vbNewLine + "Filename copied to clipboard.")

        Catch ex As Exception

            MsgBox(ex.Message)

        End Try
        'Dim objType3 = SQLResultsControl.GetType()
        'Dim field2 = objType3.GetField("m_batchConsumer", BindingFlags.NonPublic Or BindingFlags.Instance)
        'Dim batchConsumer = field2.GetValue(SQLResultsControl)

        'Dim objType4 = batchConsumer.GetType()
        'Dim field3 = objType4.GetField("m_gridContainer", BindingFlags.NonPublic Or BindingFlags.Instance)
        'Dim gridResultsPage = field3.GetValue(batchConsumer)

        'Dim Grid = GetNonPublicField(gridResultsPage, "m_grid")

        'Dim GridStorage = Grid.GridStorage

        'Dim SchemaTable = GetNonPublicField(GridStorage, "m_schemaTable")


        ''thanks - http://www.tsingfun.com/index.php?m=wap&siteid=1&c=index&a=show&catid=37&typeid=0&id=478&page=3&remains=true

        'Try

        '    Dim Filename = ExportDataSet(GridStorage, SchemaTable)

        '    MsgBox("File saved to: " + Filename + vbNewLine + "Filename copied to clipboard.")

        'Catch ex As Exception
        '    MsgBox(ex.Message)
        'End Try


    End Sub

    Private Sub RecreateTemplates()

        TemplatesList = New Hashtable

        Dim CommandBars = DirectCast(_applicationObject.CommandBars, CommandBars)
        myTemporaryToolbar = CommandBars.Item("SQL Editor") 'CommandBars.Add("SSMSAddin", MsoBarPosition.msoBarTop, System.Type.Missing, True)


        For Each Cmd2 In DirectCast(_applicationObject.Commands, Commands2)
            If Cmd2.Name.Contains("SSMSTemplates") _
                Or Cmd2.Name.Contains("SSMSRefreshTemplates") _
                Or Cmd2.Name.Contains("SSMSSettingForm") _
                Or Cmd2.Name.Contains("SSMSExportToExcel") _
                Or Cmd2.Name.Contains("SSMSFormatSelection") _
                Then
                Cmd2.Delete()
            End If
        Next
        If Not (myTemporaryPopup Is Nothing) Then
            myTemporaryPopup.Delete()
        End If

        '**************************************************************************

        Dim Folder = SettingManager.GetTemplatesFolder()

        Dim i = 1

        myTemporaryPopup = myTemporaryToolbar.Controls.Add(MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing, System.Type.Missing, True)

        myTemporaryPopup.Caption = "Templates"
        myTemporaryPopup.BeginGroup = False
        myTemporaryPopup.CommandBar.Name = "Templates"
        myTemporaryPopup.Visible = True

        'Dim Delim = myTemporaryToolbar.Controls.Add(MsoControlType.msoControlSplitDropdown, System.Type.Missing, System.Type.Missing, System.Type.Missing, True)
        'Delim.
        '***********************************************************
        'icon list
        'http://www.kebabshopblues.co.uk/2007/01/04/visual-studio-2005-tools-for-office-commandbarbutton-faceid-property/
        Dim cmd As Command = _applicationObject.Commands.AddNamedCommand(_addInInstance, "SSMSRefreshTemplates", "SSMSRefreshTemplates", "", True, 37,
                                                                 Nothing, vsCommandStatus.vsCommandStatusSupported Or vsCommandStatus.vsCommandStatusEnabled)

        Dim myToolBarButton = DirectCast(cmd.AddControl(myTemporaryPopup.CommandBar, myTemporaryPopup.CommandBar.Controls.Count + 1), CommandBarButton)

        myToolBarButton.Caption = "Refresh templates list"
        myToolBarButton.Style = MsoButtonStyle.msoButtonIconAndCaption ' It could be also msoButtonIcon
        myToolBarButton.BeginGroup = True

        '***********************************************************

        Dim cmd3 As Command = _applicationObject.Commands.AddNamedCommand(_addInInstance, "SSMSSettingForm", "SSMSSettingForm", "", True, 2946,
                                                                 Nothing, vsCommandStatus.vsCommandStatusSupported Or vsCommandStatus.vsCommandStatusEnabled)

        Dim myToolBarButton2 = DirectCast(cmd3.AddControl(myTemporaryPopup.CommandBar, myTemporaryPopup.CommandBar.Controls.Count + 1), CommandBarButton)

        myToolBarButton2.Caption = "Addin Setting"
        myToolBarButton2.Style = MsoButtonStyle.msoButtonIconAndCaption
        myToolBarButton2.BeginGroup = True
        '***********************************************************

        Dim cmd4 As Command = _applicationObject.Commands.AddNamedCommand(_addInInstance, "SSMSExportToExcel", "SSMSExportToExcel", "", True, 263,
                                                                 Nothing, vsCommandStatus.vsCommandStatusSupported Or vsCommandStatus.vsCommandStatusEnabled)

        Dim myToolBarButton4 = DirectCast(cmd4.AddControl(myTemporaryPopup.CommandBar, myTemporaryPopup.CommandBar.Controls.Count + 1), CommandBarButton)

        myToolBarButton4.Caption = "Export current grids to Excel"
        myToolBarButton4.Style = MsoButtonStyle.msoButtonIconAndCaption

        '***********************************************************

        Dim cmd5 As Command = _applicationObject.Commands.AddNamedCommand(_addInInstance, "SSMSFormatSelection", "SSMSFormatSelection", "", True, 108,
                                                                 Nothing, vsCommandStatus.vsCommandStatusSupported Or vsCommandStatus.vsCommandStatusEnabled)

        Dim myToolBarButton5 = DirectCast(cmd5.AddControl(myTemporaryPopup.CommandBar, myTemporaryPopup.CommandBar.Controls.Count + 1), CommandBarButton)

        myToolBarButton5.Caption = "Format selected document"
        myToolBarButton5.Style = MsoButtonStyle.msoButtonIconAndCaption

        '***********************************************************

        If Not String.IsNullOrEmpty(Folder) Then
            Try
                CreateCommandsInRecursion(myTemporaryPopup.CommandBar, Folder, i)
            Catch ex As Exception
                MsgBox("Error while loading script templates (SSMS Addin): " + ex.Message)
            End Try

        End If




        ' Make visible the toolbar
        myTemporaryToolbar.Visible = True


    End Sub

    Sub CreateCommandsInRecursion(ByRef Owner As CommandBar, Folder As String, ByRef i As Integer)

        Dim Dirs = My.Computer.FileSystem.GetDirectories(Folder)

        For Each DirStr In Dirs

            Dim DI = My.Computer.FileSystem.GetFileInfo(DirStr)

            Dim popup2 As CommandBarPopup = Owner.Controls.Add(MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing, 1, True)

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

            Dim cmd As Command = _applicationObject.Commands.AddNamedCommand(_addInInstance, "SSMSTemplates_" + i.ToString, FI.Name, "", True, 2687,
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

    Private Function ExportDataSet(FullFilename As String, ds As Object, SchemaTable As Object, QueryNumber As Integer) As String

        Dim newFile As New FileInfo(FullFilename)
        'If newFile.Exists Then
        'newFile.Open(FileMode.Append, FileAccess.Write)
        '    newFile = New FileInfo(FullFilename)
        'End If

        Using package As New ExcelPackage(newFile)
            Dim worksheet As ExcelWorksheet = package.Workbook.Worksheets.Add("Query_" + QueryNumber.ToString)

            Dim ColumnTypes = New Hashtable
            Dim kk = 0

            For Each Column In SchemaTable.Rows

                kk = kk + 1

                Dim TypeStr = Column(12).ToString
                Dim ColumnName = IIf(String.IsNullOrEmpty(Column(0).ToString), "(no column name)", Column(0).ToString)

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
                        worksheet.Cells(iRow + 2, iCol).Value = Val.Value
                        If TypeOf (Val.value) Is Date Then
                            worksheet.Cells(iRow + 2, iCol).Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss"
                        End If
                    End If

                Next
            Next

            worksheet.Cells.AutoFitColumns(0)

            package.Save()

        End Using




    End Function


    Private Function ExportDataSet_old(ds As Object, SchemaTable As Object) As String

        Dim mem As MemoryStream = New MemoryStream()

        Using workbook = SpreadsheetDocument.Create(mem, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook)

            Dim workbookPart = workbook.AddWorkbookPart()

            workbook.WorkbookPart.Workbook = New DocumentFormat.OpenXml.Spreadsheet.Workbook()

            workbook.WorkbookPart.Workbook.Sheets = New DocumentFormat.OpenXml.Spreadsheet.Sheets()

            ' For Each table As System.Data.DataTable In ds.Tables

            Dim sheetPart = workbook.WorkbookPart.AddNewPart(Of WorksheetPart)()
            Dim sheetData = New DocumentFormat.OpenXml.Spreadsheet.SheetData()
            sheetPart.Worksheet = New DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData)

            Dim sheets As DocumentFormat.OpenXml.Spreadsheet.Sheets = workbook.WorkbookPart.Workbook.GetFirstChild(Of DocumentFormat.OpenXml.Spreadsheet.Sheets)()
            Dim relationshipId As String = workbook.WorkbookPart.GetIdOfPart(sheetPart)

            Dim sheetId As UInteger = 1
            If sheets.Elements(Of DocumentFormat.OpenXml.Spreadsheet.Sheet)().Count() > 0 Then
                sheetId = sheets.Elements(Of DocumentFormat.OpenXml.Spreadsheet.Sheet)().[Select](Function(s) s.SheetId.Value).Max() + 1
            End If

            Dim sheet As New DocumentFormat.OpenXml.Spreadsheet.Sheet() With {.Id = relationshipId, .SheetId = sheetId, .Name = "MySheet"}
            sheets.Append(sheet)

            Dim headerRow As New DocumentFormat.OpenXml.Spreadsheet.Row()

            'Dim columns As List(Of [String]) = New List(Of String)()

            Dim ColumnTypes = New Hashtable
            Dim kk = 1

            For Each Column In SchemaTable.Rows

                Dim cell As New DocumentFormat.OpenXml.Spreadsheet.Cell()

                Dim TypeStr = Column(12).ToString
                Dim ColumnName = Column(0).ToString

                ColumnTypes.Add(kk, TypeStr)

                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.[String]
                cell.CellValue = New DocumentFormat.OpenXml.Spreadsheet.CellValue(IIf(String.IsNullOrEmpty(ColumnName), "(no column name)", ColumnName))

                headerRow.AppendChild(cell)

                kk = kk + 1
            Next

            'For Each Column In ds.ColumnNames


            'Next

            'For i = 1 To ds.TotalNumberOfColumns - 1

            '    columns.Add("Column_" + i.ToString)

            '    Dim cell As New DocumentFormat.OpenXml.Spreadsheet.Cell()
            '    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.[String]
            '    cell.CellValue = New DocumentFormat.OpenXml.Spreadsheet.CellValue("Column_" + i.ToString)
            '    headerRow.AppendChild(cell)

            'Next
            sheetData.AppendChild(headerRow)

            For iRow = 0 To ds.TotalNumberOfRows - 1

                Dim newRow As New DocumentFormat.OpenXml.Spreadsheet.Row()
                For iCol = 1 To ds.TotalNumberOfColumns - 1

                    Dim Val = ds.GetCellDataAsString(iRow, iCol)

                    Dim cell As New DocumentFormat.OpenXml.Spreadsheet.Cell()

                    Dim ColumnType = ColumnTypes.Item(iCol)

                    If ColumnType = "System.Decimal" Or ColumnType = "System.Int32" Then
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Number
                    ElseIf ColumnType = "System.DateTime" Then
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Date
                    ElseIf ColumnType = "System.Boolean" Then
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Boolean
                        'ElseIf ColumnType = "System.Decimal" Then
                        '    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Number
                    Else
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.[String]
                    End If


                    cell.CellValue = New DocumentFormat.OpenXml.Spreadsheet.CellValue(Val)

                    newRow.AppendChild(cell)

                Next
                sheetData.AppendChild(newRow)
            Next

            'Next
        End Using

        Dim Folder As String = SettingManager.GetExcelExportFolder()

        Dim FileName = Now.ToString("yyyy-MM-dd_HH-mm")

        Dim FullFilename = Path.Combine(Folder, "QueryResult_" + FileName + ".xlsx")

        My.Computer.FileSystem.WriteAllBytes(FullFilename, mem.ToArray, False)

        Dim d As New DataObject(DataFormats.Text, FullFilename)
        Clipboard.SetDataObject(d, True)

        Return FullFilename

    End Function



    Function GetNonPublicField(obj As Object, field As String) As Object

        Dim f As FieldInfo = obj.GetType().GetField(field, BindingFlags.NonPublic Or BindingFlags.Instance)
        Return f.GetValue(obj)

    End Function


End Class
