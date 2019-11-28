Imports System
Imports System.ComponentModel.Design
Imports System.Globalization
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.Shell.Interop
Imports Task = System.Threading.Tasks.Task

''' <summary>
''' Command handler
''' </summary>
Public NotInheritable Class ToolWindow1Command

    ''' <summary>
    ''' Command ID.
    ''' </summary>
    Public Const CommandId As Integer = 4129

    ''' <summary>
    ''' Command menu group (command set GUID).
    ''' </summary>
    Public Shared ReadOnly CommandSet As New Guid("4bd44899-3f53-47a0-a62e-cd04ff2663d6")

    ''' <summary>
    ''' VS Package that provides this command, not null.
    ''' </summary>
    Private ReadOnly package As AsyncPackage

    ''' <summary>
    ''' Initializes a new instance of the <see cref="ToolWindow1Command"/> class.
    ''' Adds our command handlers for menu (the commands must exist in the command table file)
    ''' </summary>
    ''' <param name="package">Owner package, not null.</param>
    Private Sub New(package As AsyncPackage, commandService As OleMenuCommandService)
        If package Is Nothing Then
            Throw New ArgumentNullException("package")
        End If

        If commandService Is Nothing Then
            Throw New ArgumentNullException(NameOf(commandService))
        End If

        Me.package = package

        Dim menuCommandId = New CommandID(CommandSet, CommandId)
        Dim menuCommand = New MenuCommand(AddressOf Me.Execute, menuCommandId)
        commandService.AddCommand(menuCommand)
    End Sub

    ''' <summary>
    ''' Gets the instance of the command.
    ''' </summary>
    Public Shared Property Instance As ToolWindow1Command

    ''' <summary>
    ''' Get service provider from the owner package.
    ''' </summary>
    Private ReadOnly Property ServiceProvider As Microsoft.VisualStudio.Shell.IAsyncServiceProvider
        Get
            Return Me.package
        End Get
    End Property

    ''' <summary>
    ''' Initializes the singleton instance of the command.
    ''' </summary>
    ''' <param name="package">Owner package, Not null.</param>
    Public Shared Async Function InitializeAsync(package As AsyncPackage) As Task
        ' Switch to the main thread - the call to AddCommand in ToolWindow1Command's constructor requires
        ' the UI thread.
        Await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken)

        Dim commandService As OleMenuCommandService = Await package.GetServiceAsync(GetType(IMenuCommandService))
        Instance = New ToolWindow1Command(package, commandService)
    End Function

    ''' <summary>
    ''' Shows the tool window when the menu item is clicked.
    ''' </summary>
    ''' <param name="sender">The event sender.</param>
    ''' <param name="e">The event args.</param>
    Private Sub Execute(sender As Object, e As EventArgs)
        ThreadHelper.ThrowIfNotOnUIThread()

        '' Get the instance number 0 of this tool window. This window Is single instance so this instance
        '' Is actually the only one.
        '' The last flag Is set to true so that if the tool window does Not exists it will be created.
        Dim window As ToolWindowPane = Me.package.FindToolWindow(GetType(ToolWindow1), 0, True)
        If window Is Nothing OrElse window.Frame Is Nothing Then
            Throw New NotSupportedException("Cannot create tool window")
        End If

        Dim windowFrame As IVsWindowFrame = window.Frame
        Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show())
    End Sub
End Class
