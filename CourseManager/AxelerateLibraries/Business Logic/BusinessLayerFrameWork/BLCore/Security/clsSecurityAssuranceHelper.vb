Imports System.Web
Imports System.Text.RegularExpressions

Namespace BLCore.Security
    ''' <summary>
    ''' This class has helper methods that are useful to overcome common security issues
    ''' </summary>
    ''' <remarks></remarks>
    '''
    Public Class clsSecurityAssuranceHelper

        Public Shared Function AvoidSQLInjectionInLiteral(ByVal Literal As String) As String
            Dim ToReturn As String = Literal.Replace("'", "")
            Return ToReturn
        End Function

        Public Shared Function AvoidLDAPInjectionInFilter(ByVal UserName As String) As String
            Dim ToReturn As String = UserName.Replace("(", "")
            ToReturn = ToReturn.Replace(")", "")
            ToReturn = ToReturn.Replace("*", "")
            ToReturn = ToReturn.Replace("|", "")
            ToReturn = ToReturn.Replace("&", "")
            Return ToReturn
        End Function

        Public Shared Function HTMLEncode(ByVal HTMLOutput As String) As String
            Return HttpUtility.HtmlEncode(HTMLOutput)
        End Function

        Public Shared Function URLEncode(ByVal URLOutput As String) As String
            Return HttpUtility.UrlEncode(URLOutput)
        End Function

        Public Shared Function AvoidXSSinHTMLFragment(ByVal HTMLOutput As String) As String
            Return HTMLOutput
        End Function

        Public Shared Function IsGUID(ByVal GUIDInput As String) As Boolean
            Dim isValid As Boolean = False
            Dim Re As Regex = New Regex("[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}", RegexOptions.IgnoreCase)
            If (Re.IsMatch(GUIDInput)) Then
                isValid = True
            End If
            Return isValid
        End Function

        Public Shared Function AvoidGUIDInjection(ByVal GUIDInput As String) As String
            If IsGUID(GUIDInput) Then
                Return GUIDInput
            Else
                Return ""
            End If
        End Function
    End Class
End Namespace