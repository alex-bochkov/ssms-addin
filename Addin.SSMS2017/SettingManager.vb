Imports System
Imports Microsoft.Win32

Namespace SettingManager

    Module SettingManagerModule

        Private Function GetRoot() As RegistryKey

            Dim SettingKeyRoot As RegistryKey = Registry.CurrentUser.CreateSubKey("Addin.SSMS2017")
            Dim testSettings As RegistryKey = SettingKeyRoot.CreateSubKey("Settings")

            Return testSettings

        End Function


        Function GetTemplatesFolder() As String

            Dim Folder As String = ""

            Try

                Dim RootKey = GetRoot()

                Folder = RootKey.GetValue("ScriptTemplatesFolder").ToString

            Catch ex As Exception
            End Try

            Return Folder

        End Function

        Function SaveTemplatesFolder(Folder As String) As Boolean

            Try

                Dim RootKey = GetRoot()

                RootKey.SetValue("ScriptTemplatesFolder", Folder)
                RootKey.Close()

            Catch ex As Exception
                Return False
            End Try

            Return True

        End Function

        Function GetExcelExportFolder() As String

            Dim Folder As String = ""

            Try

                Dim RootKey = GetRoot()

                Folder = RootKey.GetValue("ExcelExportFolder").ToString

            Catch ex As Exception
            End Try

            Return Folder

        End Function

        Function SaveExcelExportFolder(Folder As String) As Boolean

            Try

                Dim RootKey = GetRoot()

                RootKey.SetValue("ExcelExportFolder", Folder)
                RootKey.Close()

            Catch ex As Exception
                Return False
            End Try

            Return True

        End Function

        Function GetTSQLFormatVersion() As String

            Dim Folder As String = ""

            Try

                Dim RootKey = GetRoot()

                Folder = RootKey.GetValue("TSQLFormatVersion").ToString.Trim()

            Catch ex As Exception
            End Try

            Return Folder

        End Function

        Function SaveTSQLFormatVersion(Value As String) As Boolean

            Try

                Dim RootKey = GetRoot()

                RootKey.SetValue("TSQLFormatVersion", Value)
                RootKey.Close()

            Catch ex As Exception
                Return False
            End Try

            Return True

        End Function

    End Module

End Namespace