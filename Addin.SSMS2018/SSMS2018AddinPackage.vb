Imports System
Imports System.Runtime.InteropServices
Imports System.Threading
Imports Microsoft.VisualBasic
Imports Microsoft.VisualStudio.Shell
Imports Task = System.Threading.Tasks.Task


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
<PackageRegistration(UseManagedResourcesOnly:=True, AllowsBackgroundLoading:=True)>
<Guid(SSMS2018AddinPackage.PackageGuidString)>
<ProvideMenuResource("Menus.ctmenu", 1)>
<ProvideToolWindow(GetType(ToolWindow1))>
Public NotInheritable Class SSMS2018AddinPackage
    Inherits AsyncPackage

    ''' <summary>
    ''' Package guid
    ''' </summary>
    Public Const PackageGuidString As String = "eee03aee-0542-4643-892c-74a4719952eb"

    Public Sub New()

    End Sub

#Region "Package Members"

    ''' <summary>
    ''' Initialization of the package; this method is called right after the package is sited, so this is the place
    ''' where you can put all the initialization code that rely on services provided by VisualStudio.
    ''' </summary>
    ''' <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
    ''' <param name="progress">A provider for progress updates.</param>
    ''' <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
    Protected Overrides Async Function InitializeAsync(cancellationToken As CancellationToken, progress As IProgress(Of ServiceProgressData)) As Task
        ' When initialized asynchronously, the current thread may be a background thread at this point.
        ' Do any initialization that requires the UI thread after switching to the UI thread.
        Await Me.JoinableTaskFactory.SwitchToMainThreadAsync()
        Await ToolWindow1Command.InitializeAsync(Me)
    End Function

#End Region

End Class
