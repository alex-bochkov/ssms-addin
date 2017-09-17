Imports System
Imports System.ComponentModel.Design
Imports System.Diagnostics
Imports System.Globalization
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic
Imports Microsoft.VisualStudio
Imports Microsoft.VisualStudio.OLE.Interop
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.Shell.Interop
Imports Microsoft.Win32

''' <summary>
''' This is the class that implements the package exposed by this assembly.
''' </summary>
''' <remarks>
''' <para>
''' The minimum requirement for a class to be considered a valid package for Visual Studio
''' Is to implement the IVsPackage interface And register itself with the shell.
''' This package uses the helper classes defined inside the Managed Package Framework (MPF)
''' to do it: it derives from the Package Class that provides the implementation Of the 
''' IVsPackage interface And uses the registration attributes defined in the framework to 
''' register itself And its components with the shell. These attributes tell the pkgdef creation
''' utility what data to put into .pkgdef file.
''' </para>
''' <para>
''' To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
''' </para>
''' </remarks>
<PackageRegistration(UseManagedResourcesOnly:=True)>
<InstalledProductRegistration("#110", "#112", "1.0", IconResourceID:=400)>
<ProvideMenuResource("Menus.ctmenu", 1)>
<Guid(AddinSSMS2017Package.PackageGuidString)>
<ProvideAutoLoad(Microsoft.VisualStudio.VSConstants.UICONTEXT.NoSolution_string)>
Public NotInheritable Class AddinSSMS2017Package
    Inherits Package

    ''' <summary>
    ''' Package guid
    ''' </summary>
    Public Const PackageGuidString As String = "08f29958-5746-4168-b0f1-43b0167b0c40"

    ''' <summary>
    ''' Default constructor of the package.
    ''' Inside this method you can place any initialization code that does not require 
    ''' any Visual Studio service because at this point the package object is created but 
    ''' not sited yet inside Visual Studio environment. The place to do all the other 
    ''' initialization is the Initialize method.
    ''' </summary>
    Public Sub New()

    End Sub

#Region "Package Members"


    Private Const PackageGuidGroup As String = "08f29958-5746-4168-b0f1-43b0167b0c40"

    Private cmdId1 As Integer
    Private cmdId2 As Integer


    ''' <summary>
    ''' Initialization of the package; this method is called right after the package is sited, so this is the place
    ''' where you can put all the initialization code that rely on services provided by VisualStudio.
    ''' </summary>
    Protected Overrides Sub Initialize()
        MainClass.Initialize(Me)
        MyBase.Initialize()

        Dim a = 1

        Dim profferCommands3 As IVsProfferCommands3
        Dim oleMenuCommandService As OleMenuCommandService

        Try

            profferCommands3 = TryCast(MyBase.GetService(GetType(SVsProfferCommands)), IVsProfferCommands3)

            oleMenuCommandService = TryCast(GetService(GetType(IMenuCommandService)), OleMenuCommandService)

            cmdId1 = AddCommand(profferCommands3, oleMenuCommandService, "MyCommand1", "My Command 1 Caption", "My Command 1 Tooltip")
            cmdId2 = AddCommand(profferCommands3, oleMenuCommandService, "MyCommand2", "My Command 2 Caption", "My Command 2 Tooltip")
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine(ex.ToString())
        End Try

    End Sub


    Private Function AddCommand(profferCommands3 As IVsProfferCommands3, oleMenuCommandService As OleMenuCommandService, commandName As String, commandCaption As String, commandTooltip As String) As Integer
        Const CMD_FLAGS As UInteger = 0
        Const SATELLITE_DLL As String = ""
        Const BITMAP_RESOURCE_ID As UInteger = 0
        Const BITMAP_IMAGE_INDEX As UInteger = 0
        Const UI_CONTEXTS As Integer = 0
        Dim GUID_UI_CONTEXTS As Guid() = Nothing

        Dim packageGuid As New Guid(PackageGuidString)
        Dim cmdGroupGuid As New Guid(PackageGuidGroup)
        Dim result As Integer
        Dim commandID As CommandID
        Dim oleMenuCommand As OleMenuCommand
        Dim cmdId As UInteger = 0

        result = profferCommands3.AddNamedCommand2(packageGuid, cmdGroupGuid, commandName, cmdId, commandName, commandCaption,
            commandTooltip, SATELLITE_DLL, BITMAP_RESOURCE_ID, BITMAP_IMAGE_INDEX, CMD_FLAGS, UI_CONTEXTS,
            GUID_UI_CONTEXTS, EnvDTE80.vsCommandControlType.vsCommandControlTypeButton)

        ' Note: the result can be:
        ' 1) Microsoft.VisualStudio.VSConstants.S_OK. This should be the case of the first creation, when the command didn't exist previously
        ' 2) Microsoft.VisualStudio.VSConstants.S_FALSE. This can be because:
        '   2.1) The command with that name already exists. In this case the returned cmdId is valid.
        '   2.2) There is some error. In this case the returned cmdId remains with value 0.

        If cmdId <> 0 Then
            If oleMenuCommandService IsNot Nothing Then
                commandID = New CommandID(cmdGroupGuid, CInt(cmdId))

                oleMenuCommand = New OleMenuCommand(AddressOf OleMenuCommandCallback, commandID)
                AddHandler oleMenuCommand.BeforeQueryStatus, AddressOf Me.OleMenuCommandBeforeQueryStatus
                oleMenuCommandService.AddCommand(oleMenuCommand)
            End If
        Else
            Throw New ApplicationException("Failed to add command")
        End If
        Return CInt(cmdId)
    End Function

    Private Sub OleMenuCommandBeforeQueryStatus(sender As Object, e As EventArgs)
        Dim oleMenuCommand As OleMenuCommand
        Dim commandId As CommandID

        Try
            oleMenuCommand = TryCast(sender, OleMenuCommand)

            If oleMenuCommand IsNot Nothing Then
                commandId = oleMenuCommand.CommandID

                If commandId IsNot Nothing Then
                    If commandId.ID = cmdId1 Then
                        oleMenuCommand.Supported = True
                        oleMenuCommand.Enabled = True
                        oleMenuCommand.Visible = True
                    ElseIf commandId.ID = cmdId2 Then
                        oleMenuCommand.Supported = True
                        oleMenuCommand.Enabled = True
                        oleMenuCommand.Visible = True
                    End If
                End If
            End If
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine(ex.ToString())
        End Try
    End Sub

    Private Sub OleMenuCommandCallback(sender As Object, e As EventArgs)
        Dim oleMenuCommand As OleMenuCommand
        Dim commandId As CommandID

        Try
            oleMenuCommand = TryCast(sender, OleMenuCommand)

            If oleMenuCommand IsNot Nothing Then
                commandId = oleMenuCommand.CommandID
                If commandId IsNot Nothing Then
                    System.Windows.Forms.MessageBox.Show("Executed command with Id = " + commandId.ID.ToString())
                End If
            End If
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine(ex.ToString())
        End Try
    End Sub

#End Region

End Class
