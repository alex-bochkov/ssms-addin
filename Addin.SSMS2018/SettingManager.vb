Imports System
Imports System.IO
Imports Microsoft.Win32

Namespace SettingManager

    Module SettingManagerModule

        Private Function GetRoot() As RegistryKey

            Dim SettingKeyRoot As RegistryKey = Registry.CurrentUser.CreateSubKey("DBA-Helper.SSMS2018")
            Dim testSettings As RegistryKey = SettingKeyRoot.CreateSubKey("Settings")

            Return testSettings

        End Function

        Private Function GetRegisterValue(Parameter As String) As String

            Dim RegisterValue As String = ""

            Try

                Dim RootKey = GetRoot()

                RegisterValue = RootKey.GetValue(Parameter).ToString

            Catch ex As Exception
            End Try

            Return RegisterValue

        End Function
        Private Function SaveRegisterValue(ParameterName As String, ParameterValue As String) As Boolean

            Try

                Dim RootKey = GetRoot()

                RootKey.SetValue(ParameterName, ParameterValue)
                RootKey.Close()

            Catch ex As Exception
                Return False
            End Try

            Return True

        End Function

        '***********************************************************************************

        Function GetTemplatesFolder() As String

            Dim Folder As String = GetRegisterValue("ScriptTemplatesFolder")

            If String.IsNullOrEmpty(Folder) Then

                Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dba-helper-scripts")

            End If

            Return Folder

        End Function

        Function GetSQLParserVersion() As String

            Dim TSQLParserVersion As String = GetRegisterValue("TSQLParserVersion")

            If String.IsNullOrEmpty(TSQLParserVersion) Then

                TSQLParserVersion = "SQL Server 2017" 'default value

            End If

            Return TSQLParserVersion

        End Function

        Function GetSQLParserType() As Boolean

            Dim TSQLParserType As String = GetRegisterValue("TSQLParserType")

            If String.IsNullOrEmpty(TSQLParserType) Then

                Return False

            End If

            If TSQLParserType = "False" Then
                Return False
            Else
                Return True
            End If

        End Function

        Function SaveTemplatesFolder(Folder As String) As Boolean

            Return SaveRegisterValue("ScriptTemplatesFolder", Folder)

        End Function

        Function SaveSQLParserVersion(TSQLParserVersion As String) As Boolean

            Return SaveRegisterValue("TSQLParserVersion", TSQLParserVersion)

        End Function

        Function SaveSQLParserType(TSQLParserType As Boolean) As Boolean

            Return SaveRegisterValue("TSQLParserType", TSQLParserType.ToString())

        End Function

        'Function GetExcelExportFolder() As String

        '    Dim Folder As String = ""

        '    Try

        '        Dim RootKey = GetRoot()

        '        Folder = RootKey.GetValue("ExcelExportFolder").ToString

        '    Catch ex As Exception
        '    End Try

        '    Return Folder

        'End Function

        'Function SaveExcelExportFolder(Folder As String) As Boolean

        '    Try

        '        Dim RootKey = GetRoot()

        '        RootKey.SetValue("ExcelExportFolder", Folder)
        '        RootKey.Close()

        '    Catch ex As Exception
        '        Return False
        '    End Try

        '    Return True

        'End Function

    End Module

End Namespace