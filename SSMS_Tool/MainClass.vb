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

Public Class Connect

    Implements IDTExtensibility2
    Implements IDTCommandTarget

    Private _applicationObject As DTE2
    Private _addInInstance As AddIn
    Dim myTemporaryToolbar As CommandBar
    Dim myTemporaryPopup As CommandBarPopup

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
            Dim contextGUIDS As Object() = New Object() {}
            Dim commands As Commands2 = DirectCast(_applicationObject.Commands, Commands2)
            Dim toolsMenuName As String = "Tools"
            Dim menuBarCommandBar As CommandBar = (DirectCast(_applicationObject.CommandBars, Microsoft.VisualStudio.CommandBars.CommandBars))("MenuBar")
            Dim toolsControl As CommandBarControl = menuBarCommandBar.Controls(toolsMenuName)
            Dim toolsPopup As CommandBarPopup = DirectCast(toolsControl, CommandBarPopup)
            Try

                For Each Cmd As Command In commands
                    If Cmd.Name.Contains("SSMSAddin") Then
                        Cmd.Delete()
                    End If
                Next

                Dim command As Command = commands.AddNamedCommand2(_addInInstance, "SSMSAddin", "SSMSAddin", "Executes the command for SSMSAddin", True, 59,
                    contextGUIDS, DirectCast(vsCommandStatus.vsCommandStatusSupported, Integer) + DirectCast(vsCommandStatus.vsCommandStatusEnabled, Integer), DirectCast(vsCommandStyle.vsCommandStylePictAndText, Integer), vsCommandControlType.vsCommandControlTypeButton)
                If (Not command Is Nothing) AndAlso (Not toolsPopup Is Nothing) Then
                    command.AddControl(toolsPopup.CommandBar, 1)
                End If
            Catch generatedExceptionName As System.ArgumentException
                MsgBox(generatedExceptionName.Message)
            End Try

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

            If commandName.Contains("SSMSTemplates") Then

                Dim document As Document = (DirectCast(ServiceCache.ExtensibilityModel, DTE2)).ActiveDocument

                Dim TemplateFile = TemplatesList.Item(commandName)
                If Not TemplateFile Is Nothing Then

                    Dim Text = My.Computer.FileSystem.ReadAllText(TemplateFile)

                    Dim txt As TextDocument = CType(document.Object("TextDocument"), TextDocument)

                    'get an edit point
                    Dim ep As EditPoint = txt.Selection.ActivePoint.CreateEditPoint

                    'get a start point
                    Dim sp As EditPoint = txt.Selection.ActivePoint.CreateEditPoint

                    ''open the undo context
                    'Dim isOpen As Boolean = Application.UndoContext.IsOpen
                    'If Not isOpen Then Application.UndoContext.Open("SmartPaster", False)

                    'clear the selection
                    If Not txt.Selection.IsEmpty Then txt.Selection.Delete()

                    'insert the text
                    'ep.Insert(Indent(text, ep.LineCharOffset))
                    ep.Insert(Text)

                    'smart format
                    'If Configuration.AutoFormatAfterPaste Then
                    'sp.SmartFormat(ep)
                    'End If




                End If



            ElseIf commandName = "SSMSTool.Connect.SSMSAddin" Then
                    Dim document As Document = (DirectCast(ServiceCache.ExtensibilityModel, DTE2)).ActiveDocument


                    'Dim sqlScriptEditorControl As Object = InvokeMethod(ServiceCache.ScriptFactory, "GetCurrentlyActiveFrameDocView", BindingFlags.NonPublic Or BindingFlags.Instance, New Object() {ServiceCache.VSMonitorSelection, False, Nothing})
                    ' m_SQLResultsControl = GetField(sqlScriptEditorControl, "m_sqlResultsControl", BindingFlags.NonPublic Or BindingFlags.Instance)

                    Dim objType = ServiceCache.ScriptFactory.GetType()
                    Dim method1 = objType.GetMethod("GetCurrentlyActiveFrameDocView", BindingFlags.NonPublic Or BindingFlags.Instance)
                    Dim Result = method1.Invoke(ServiceCache.ScriptFactory, New Object() {ServiceCache.VSMonitorSelection, False, Nothing})

                    Dim objType2 = Result.GetType()
                    Dim field = objType2.GetField("m_sqlResultsControl", BindingFlags.NonPublic Or BindingFlags.Instance)
                    Dim SQLResultsControl = field.GetValue(Result)

                    'Dim gridContainers As CollectionBase = GetNonPublicField(gridResultsPage, "m_gridContainers")

                    'Dim ienum As IEnumerator = gridContainers.GetEnumerator()
                    'ienum.MoveNext()

                    'Dim gridResultGrid As Object = GetNonPublicField(ienum.Current, "m_grid")
                    ''Return gridResultGrid As IGridControl;
                    'Dim gs As Object = gridResultGrid.GridStorage
                    'Dim Text = gs.GetCellDataAsString(0, 1)

                    Dim objType3 = SQLResultsControl.GetType()
                    Dim field2 = objType3.GetField("m_batchConsumer", BindingFlags.NonPublic Or BindingFlags.Instance)
                    Dim batchConsumer = field2.GetValue(SQLResultsControl)

                    Dim objType4 = batchConsumer.GetType()
                    Dim field3 = objType4.GetField("m_gridContainer", BindingFlags.NonPublic Or BindingFlags.Instance)
                    Dim gridResultsPage = field3.GetValue(batchConsumer)

                    Dim Grid = GetNonPublicField(gridResultsPage, "m_grid")

                    Dim GridStorage = Grid.GridStorage
                    'Grid.BackColor = Drawing.Color.Red

                    'Dim Text = GridStorage.GetCellDataAsString(0, 1)

                    Dim Str = ""

                    'Dim iRow = GridStorage.TotalNumberOfRows - 1
                    'Dim iCol = GridStorage.TotalNumberOfColumns - 1

                    'thanks - http://www.tsingfun.com/index.php?m=wap&siteid=1&c=index&a=show&catid=37&typeid=0&id=478&page=3&remains=true

                    For iRow = 0 To GridStorage.TotalNumberOfRows - 1
                        For iCol = 1 To GridStorage.TotalNumberOfColumns - 1
                            Str = Str + GridStorage.GetCellDataAsString(iRow, iCol)
                        Next
                        Str = Str + vbNewLine
                    Next


                    Try

                        ExportDataSet(GridStorage)

                        MsgBox("File saved and copied to clipboard!")

                    Catch ex As Exception
                        MsgBox(ex.Message)
                    End Try





                    'If Not document Is Nothing Then
                    '    Dim selection As TextSelection = DirectCast(document.Selection, TextSelection)
                    '    selection.Insert("Welcome to SSMS! This sample is brought to you by SSMSBoost add-in team.", DirectCast(vsInsertFlags.vsInsertFlagsContainNewText, Int32))
                    'End If
                    handled = True
                    Return
                End If
            End If

    End Sub

    Private Sub RecreateTemplates()

        TemplatesList = New Hashtable

        Dim CommandBars = DirectCast(_applicationObject.CommandBars, CommandBars)
        myTemporaryToolbar = CommandBars.Item("SQL Editor") 'CommandBars.Add("SSMSAddin", MsoBarPosition.msoBarTop, System.Type.Missing, True)


        For Each Cmd2 In DirectCast(_applicationObject.Commands, Commands2)
            If Cmd2.Name.Contains("SSMSTemplates") Then
                Cmd2.Delete()
            End If
        Next

        Dim Folder = "C:\Users\abochkov\Source\Repos\ssms-addin\SSMS_Tool\QueryTemplates"
        Dim i = 1

        myTemporaryPopup = myTemporaryToolbar.Controls.Add(MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing, System.Type.Missing, True)

        myTemporaryPopup.Caption = "Templates"
        myTemporaryPopup.BeginGroup = False
        myTemporaryPopup.CommandBar.Name = "Templates"
        myTemporaryPopup.Visible = True

        CreateCommandsInRecursion(myTemporaryPopup.CommandBar, Folder, i)



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

            Dim cmd As Command = _applicationObject.Commands.AddNamedCommand(_addInInstance, "SSMSTemplates_" + i.ToString, FI.Name, "", True, ,
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


    Private Sub ExportDataSet(ds As Object)

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

            Dim columns As List(Of [String]) = New List(Of String)()
            For i = 1 To ds.TotalNumberOfColumns - 1

                columns.Add("Column_" + i.ToString)

                Dim cell As New DocumentFormat.OpenXml.Spreadsheet.Cell()
                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.[String]
                cell.CellValue = New DocumentFormat.OpenXml.Spreadsheet.CellValue("Column_" + i.ToString)
                headerRow.AppendChild(cell)

            Next
            sheetData.AppendChild(headerRow)

            For iRow = 0 To ds.TotalNumberOfRows - 1

                Dim newRow As New DocumentFormat.OpenXml.Spreadsheet.Row()
                For iCol = 1 To ds.TotalNumberOfColumns - 1
                    Dim Val = ds.GetCellDataAsString(iRow, iCol)

                    Dim cell As New DocumentFormat.OpenXml.Spreadsheet.Cell()
                    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.[String]
                    cell.CellValue = New DocumentFormat.OpenXml.Spreadsheet.CellValue(Val)
                    '
                    newRow.AppendChild(cell)
                Next
                sheetData.AppendChild(newRow)
            Next

            'Next
        End Using

        Dim f As String = "C:\temp\excel_export.xlsx"

        My.Computer.FileSystem.WriteAllBytes(f, mem.ToArray, False)


        'Dim d As New DataObject(DataFormats.FileDrop, f)
        'Clipboard.SetDataObject(d, True)

    End Sub



    Function GetNonPublicField(obj As Object, field As String) As Object

        Dim f As FieldInfo = obj.GetType().GetField(field, BindingFlags.NonPublic Or BindingFlags.Instance)
        Return f.GetValue(obj)

    End Function


End Class
