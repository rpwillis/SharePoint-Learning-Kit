Imports System.Configuration.ConfigurationManager

''' <summary>
''' Class the defines some methods that will be useful for configuration purposes
''' </summary>
''' <remarks></remarks>
Public Class ConfigurationHelper
    Public Shared ReadOnly Property AppSetting(ByVal SettingName As String) As String
        Get

            Dim Found As Boolean = False
            For Each key As String In AppSettings.Keys
                If (key = SettingName) Then
                    Found = True
                    Exit For
                End If
            Next

            If (Found) Then
                Return AppSettings(SettingName)
            End If
            Return ""
        End Get
    End Property

    Public Shared ReadOnly Property BoolAppSetting(ByVal SettingName As String) As Boolean
        Get
            Dim SettingValue As String = AppSetting(SettingName)
            If SettingValue <> "" Then
                Return CType(SettingValue, Boolean)
            End If
            Return False
        End Get
    End Property


End Class
