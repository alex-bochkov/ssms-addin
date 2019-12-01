Imports System
Imports System.Collections
Imports System.ComponentModel
Imports System.Drawing
Imports System.Data
Imports System.Windows
Imports System.Runtime.InteropServices
Imports Microsoft.VisualStudio.Shell.Interop
Imports Microsoft.VisualStudio.Shell

''' <summary>
''' This class implements the tool window exposed by this package and hosts a user control.
''' </summary>
''' <remarks>
''' In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
''' usually implemented by the package implementer.
''' <para>
''' This class derives from the ToolWindowPane class provided from the MPF in order to use its 
''' implementation of the IVsUIElementPane interface.
''' </para>
''' </remarks>
<Guid("91e44dc2-9c96-4488-9fa7-9532655f1650")>
Public Class ToolWindow1
    Inherits ToolWindowPane

    ''' <summary>
    ''' Initializes a new instance of the <see cref="ToolWindow1"/> class.
    ''' </summary>
    Public Sub New()
        MyBase.New(Nothing)
        Me.Caption = "DBA Helper"

        'This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
        'we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
        'the object returned by the Content property.
        Me.Content = New ToolWindow1Control()
    End Sub

End Class
