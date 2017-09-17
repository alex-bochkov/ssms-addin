''------------------------------------------------------------------------------
'' <copyright file="SSMSAddinFactory.vb" company="Company">
''     Copyright (c) Company.  All rights reserved.
'' </copyright>
''------------------------------------------------------------------------------

Imports System.ComponentModel.Composition
Imports Microsoft.VisualStudio.Text.Editor
Imports Microsoft.VisualStudio.Utilities

''' <summary>
''' Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor to use.
''' </summary>
<Export(GetType(IWpfTextViewMarginProvider))>
<Name(SSMSAddin.MarginName)>
<MarginContainer(PredefinedMarginNames.Top)>
<ContentType("sql")>
<TextViewRole(PredefinedTextViewRoles.Interactive)>
Friend NotInheritable Class SSMSAddinFactory
    Implements IWpfTextViewMarginProvider

    Public Sub New()
        Dim a = 0

    End Sub

#Region "IWpfTextViewMarginProvider"

    ''' <summary>
    ''' Creates an <see cref="IWpfTextViewMargin"/> for the given <see cref="IWpfTextViewHost"/>.
    ''' </summary>
    ''' <param name="wpfTextViewHost">The <see cref="IWpfTextViewHost"/> for which to create the <see cref="IWpfTextViewMargin"/>.</param>
    ''' <param name="marginContainer">The margin that will contain the newly-created margin.</param>
    ''' <returns>The <see cref="IWpfTextViewMargin"/>.
    ''' The value may be null if this <see cref="IWpfTextViewMarginProvider"/> does Not participate for this context.
    ''' </returns>
    Public Function CreateMargin(ByVal wpfTextViewHost As IWpfTextViewHost, ByVal marginContainer As IWpfTextViewMargin) As IWpfTextViewMargin Implements IWpfTextViewMarginProvider.CreateMargin

        Return New SSMSAddin(wpfTextViewHost.TextView)

    End Function

#End Region

End Class
