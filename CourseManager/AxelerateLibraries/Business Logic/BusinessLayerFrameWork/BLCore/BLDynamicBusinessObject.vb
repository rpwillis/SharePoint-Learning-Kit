Namespace BLCore

    <DataLayerInfo("DataLayerDefinition.xml", True)> _
    Public Class BLDynamicBusinessObject
        Inherits BLCore.Templates.BLBusinessBase(Of BLDynamicBusinessObject, DynamicDataKey)

#Region "DataKey"
        Public Class DynamicDataKey
            Inherits BLDataKey

            <DynamicDataKey()> Public m_DataKeyValues As New ArrayList

            Public Sub New(Optional ByVal DataLayerContext As DataLayerContextInfo = Nothing)
                MyBase.New(GetType(BLDynamicBusinessObject), DataLayerContext)
            End Sub

            Public Overrides ReadOnly Property NewDataKey() As BLDataKey
                Get
                    Return New DynamicDataKey(m_DataLayerContext)
                End Get
            End Property
        End Class

        Public Overrides ReadOnly Property DataKey() As BLCore.BLDataKey
            Get
                Dim NDataKey As New DynamicDataKey(DataLayerContextInfo)
                For Each Field As BLFieldMap In DataLayer.FieldMapList.KeyFields
                    If Field.FieldMapType = BLFieldMap.FieldMapTypeEnum.DynamicDataField Then
                        Dim Pair As New BLCore.DataTypes.Pair(Of String, Object)(Field.DLFieldName, FieldValue(Field))
                        NDataKey.m_DataKeyValues.Add(Pair)
                    End If
                Next
                Return NDataKey
            End Get
        End Property

#End Region


    End Class

End Namespace