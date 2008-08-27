Namespace DLCore



    ''' <summary>
    ''' Class that represents a filter of values capable of changing the content
    ''' of the information during the command execution
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public MustInherit Class DataLayerFilterBase
        'Returns for each specific collection a valid sentence of the selection.
        'This sentence must select the fields of the table that map the ones in the object
        Public MustOverride Function SelectCommandText(ByVal DataLayer As DataLayerAbstraction, ByVal FieldMapList As BLFieldMapList, ByVal AditionalFilter As String, ByRef ParameterList As List(Of DataLayerParameter)) As String

        Protected Sub AddAditionalFilter(ByRef TextSentence As String, ByVal AditionalFilter As String)
            If AditionalFilter <> "" Then
                TextSentence = TextSentence + " AND (" + AditionalFilter + ")"
            End If

        End Sub

        Protected Sub AddAditionalFilter(ByRef TextSentence As String, ByVal AditionalFilter As String, ByVal ConcatenationOperator As String)
            If AditionalFilter <> "" Then
                TextSentence = TextSentence + " " + ConcatenationOperator + " (" + AditionalFilter + ")"
            End If

        End Sub


    End Class
End Namespace