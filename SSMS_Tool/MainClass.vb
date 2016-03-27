Imports System
Imports Extensibility
Imports EnvDTE
Imports EnvDTE80
Imports Microsoft.VisualStudio.CommandBars
Imports Microsoft.SqlServer.Management.UI.VSIntegration
Imports System.Resources
Imports System.Reflection
Imports System.Management
Imports System.Globalization
Imports Microsoft.SqlServer.Management
Imports System.Windows.Forms

Public Class Connect

    Implements IDTExtensibility2
    Implements IDTCommandTarget

    Private _applicationObject As DTE2
    Private _addInInstance As AddIn

    Private QueryExecuteEvent As CommandEvents

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

        End If

    End Sub
    Public Sub OnDisconnection(disconnectMode As ext_DisconnectMode, ByRef [custom] As Array) Implements IDTExtensibility2.OnDisconnection
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
            End If
        End If

    End Sub
    Public Sub Exec(commandName As String, executeOption As vsCommandExecOption, ByRef varIn As Object, ByRef varOut As Object, ByRef handled As Boolean) Implements IDTCommandTarget.Exec

        handled = False
        If executeOption = vsCommandExecOption.vsCommandExecOptionDoDefault Then
            If commandName = "SSMSTool.Connect.SSMSAddin" Then
                Dim document As Document = (DirectCast(ServiceCache.ExtensibilityModel, DTE2)).ActiveDocument


                'Dim sqlScriptEditorControl As Object = InvokeMethod(ServiceCache.ScriptFactory, "GetCurrentlyActiveFrameDocView", BindingFlags.NonPublic Or BindingFlags.Instance, New Object() {ServiceCache.VSMonitorSelection, False, Nothing})
                ' m_SQLResultsControl = GetField(sqlScriptEditorControl, "m_sqlResultsControl", BindingFlags.NonPublic Or BindingFlags.Instance)

                Dim objType = ServiceCache.ScriptFactory.GetType()
                Dim method1 = objType.GetMethod("GetCurrentlyActiveFrameDocView", BindingFlags.NonPublic Or BindingFlags.Instance)
                Dim Result = method1.Invoke(ServiceCache.ScriptFactory, New Object() {ServiceCache.VSMonitorSelection, False, Nothing})

                Dim objType2 = Result.GetType()
                Dim field = objType2.GetField("m_sqlResultsControl", BindingFlags.NonPublic Or BindingFlags.Instance)
                Dim SQLResultsControl = field.GetValue(Result)

                Dim objType3 = SQLResultsControl.GetType()
                Dim field2 = objType3.GetField("m_batchConsumer", BindingFlags.NonPublic Or BindingFlags.Instance)
                Dim batchConsumer = field2.GetValue(SQLResultsControl)

                Dim objType4 = batchConsumer.GetType()
                Dim field3 = objType4.GetField("m_gridContainer", BindingFlags.NonPublic Or BindingFlags.Instance)
                Dim gridResultsPage = field3.GetValue(batchConsumer)

                Dim Grid = gridResultsPage.Grid
                Grid.BackColor = Drawing.Color.Red



                If Not document Is Nothing Then
                    Dim selection As TextSelection = DirectCast(document.Selection, TextSelection)
                    selection.Insert("Welcome to SSMS! This sample is brought to you by SSMSBoost add-in team.", DirectCast(vsInsertFlags.vsInsertFlagsContainNewText, Int32))
                End If
                handled = True
                Return
            End If
        End If

    End Sub

    Function GetNonPublicField(obj As Object, field As String) As Object

        Dim f As FieldInfo = obj.GetType().GetField(field, BindingFlags.NonPublic Or BindingFlags.Instance)
        Return f.GetValue(obj)

    End Function


End Class
