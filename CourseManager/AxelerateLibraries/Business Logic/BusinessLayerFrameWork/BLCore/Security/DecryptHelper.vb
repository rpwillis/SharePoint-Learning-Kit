Namespace BLCore.Security

    ''' <summary>
    ''' Decrypts the connection string of the database, stored in the configuration file of the application.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DecryptHelper
        ''' <summary>
        ''' Decrypts a carater
        ''' </summary>
        ''' <param name="TwoCharString"></param>
        ''' <param name="StringPosition"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' </remarks>
        Public Shared Function DecryptChar(ByVal TwoCharString As String, ByVal StringPosition As Long) As Char
            Dim ToReturn As Char
            Dim RandNumber As Integer
            If (StringPosition Mod 2) = 0 Then
                RandNumber = CInt(TwoCharString.Chars(1).ToString)
                ToReturn = Chr(Asc(TwoCharString.Chars(0)) - RandNumber)
            Else
                RandNumber = CInt(TwoCharString.Chars(0).ToString)
                ToReturn = Chr(Asc(TwoCharString.Chars(1)) + RandNumber)
            End If

            Return ToReturn
        End Function
        ''' <summary>
        ''' Decrypts a text returns (ByRef), in UserName and Password the values
        ''' regarding the username and password of the connection string
        ''' </summary>
        ''' <param name="EncryptedString"></param>
        ''' <param name="UserName"></param>
        ''' <param name="Password"></param>
        ''' <remarks>
        ''' </remarks>
        Public Shared Sub DecryptString(ByVal EncryptedString As String, ByRef UserName As String, ByRef Password As String)
            Dim i As Integer

            Dim RandChars As Integer = CInt(EncryptedString.Chars(0).ToString)
            Dim RandCharsPos() As Integer
            Dim LastRandCharPos As Integer = 0
            Dim ActualRandChar As Integer = 0
            ReDim RandCharsPos(RandChars)
            RandCharsPos(0) = CInt(EncryptedString.Chars(1).ToString)

            Dim ActualPos As Integer = 2
            Dim ActualDecryptedPos As Integer = 0

            Dim TotalString As String = ""


            While ActualPos < EncryptedString.Length
                If ActualRandChar < RandChars AndAlso RandCharsPos(ActualRandChar) = ActualDecryptedPos Then
                    If ActualRandChar < RandChars Then
                        RandCharsPos(ActualRandChar + 1) = CInt(EncryptedString.Chars(ActualPos + 1).ToString)
                    End If
                    ActualRandChar = ActualRandChar + 1
                    ActualPos = ActualPos + 2
                End If

                If ActualPos < EncryptedString.Length Then

                    Dim TwoCharString As String = EncryptedString.Chars(ActualPos).ToString + EncryptedString.Chars(ActualPos + 1).ToString
                    TotalString = TotalString + DecryptChar(TwoCharString, ActualDecryptedPos).ToString
                    ActualDecryptedPos = ActualDecryptedPos + 1

                    ActualPos = ActualPos + 2
                End If

            End While

            UserName = Mid(TotalString, 1, TotalString.IndexOf("{-}"))
            Password = Mid(TotalString, TotalString.IndexOf("{-}") + 4)
        End Sub
        ''' <summary>
        ''' Takes the connection string and looks for the location of the username and the password. 
        ''' Once localized reads them, decrypts them using DecryptString(EncryptedUserName, UserName, Password)
        ''' and replaces in the text the values that the connection string has encrypted
        ''' </summary>
        ''' <param name="ConnectionString"></param>
        ''' <returns>The connection string</returns>
        ''' <remarks>
        ''' </remarks>
        Public Shared Function DecryptConnectionString(ByVal ConnectionString As String) As String
            Dim UserNameInitPos As Integer = ConnectionString.IndexOf("[[[") + 4
            Dim UserNameEndPos As Integer = ConnectionString.IndexOf("]]]")
            Dim EncryptedUserName As String = Mid(ConnectionString, UserNameInitPos, UserNameEndPos - UserNameInitPos + 1)

            Dim PasswordInitPos As Integer = ConnectionString.IndexOf("{{{") + 4
            Dim PasswordEndPos As Integer = ConnectionString.IndexOf("}}}")
            Dim EncryptedPassword As String = Mid(ConnectionString, PasswordInitPos, PasswordEndPos - PasswordInitPos + 1)

            Dim UserName As String
            Dim Password As String

            DecryptString(EncryptedUserName, UserName, Password)

            ConnectionString = ConnectionString.Replace("[[[" + EncryptedUserName + "]]]", UserName)
            ConnectionString = ConnectionString.Replace("{{{" + EncryptedPassword + "}}}", Password)


            Return ConnectionString
        End Function
    End Class
End Namespace