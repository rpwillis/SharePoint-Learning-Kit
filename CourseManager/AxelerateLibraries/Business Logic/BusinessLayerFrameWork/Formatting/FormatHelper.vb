Public Class FormatHelper

    Private Shared Culture As System.Globalization.CultureInfo
    Private Shared CurrencySymbol As String


    Shared Sub New()
        Culture = System.Globalization.CultureInfo.CurrentCulture
        CurrencySymbol = Culture.NumberFormat.CurrencySymbol

    End Sub
    Public Shared Function MoneyToString(ByVal Amount As Double) As String
        Dim Culture As System.Globalization.CultureInfo
        Dim FormatedString As String = Microsoft.VisualBasic.Format(Amount, "Currency")
        FormatedString = FormatedString.Remove(FormatedString.IndexOf(CurrencySymbol), CurrencySymbol.Length)
        Return FormatedString
    End Function

End Class
