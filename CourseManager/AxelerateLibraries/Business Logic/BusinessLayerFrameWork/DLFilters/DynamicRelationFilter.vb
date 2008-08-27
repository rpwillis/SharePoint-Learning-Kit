Namespace DLFilters
    Public Class SQLDynamicRelationFilter
        Inherits DLCore.DataLayerFilterBase

        Private m_MasterDataLayerContextInfo As DataLayerContextInfo
        Private m_DetailDataLayerContextInfo As DataLayerContextInfo
        Private m_MasterKeyFieldName As String
        Private m_DetailForeignKeyFieldName As String


        Public Sub New(ByVal MasterDataLayerContextInfo As DataLayerContextInfo, ByVal DetailDataLayerContextInfo As DataLayerContextInfo, _
        ByVal MasterKeyFieldName As String, ByVal DetailForeignKeyFieldName As String)
            m_MasterDataLayerContextInfo = MasterDataLayerContextInfo
            m_DetailDataLayerContextInfo = DetailDataLayerContextInfo
            m_MasterKeyFieldName = MasterKeyFieldName
            m_DetailForeignKeyFieldName = DetailForeignKeyFieldName
        End Sub


        Public Overrides Function SelectCommandText(ByVal DataLayer As DataLayerAbstraction, ByVal FieldMapList As BLCore.BLFieldMapList, ByVal AditionalFilter As String, ByRef ParameterList As System.Collections.Generic.List(Of DLCore.DataLayerParameter)) As String
            Dim MasterDataLayer As SQLDataLayer = CType(DataLayerCache.DataLayer(m_MasterDataLayerContextInfo), SQLDataLayer)
            Dim DetailDataLayer As SQLDataLayer = CType(DataLayerCache.DataLayer(m_DetailDataLayerContextInfo), SQLDataLayer)

            Dim NSelectSQL As String
            NSelectSQL = "SELECT " + DetailDataLayer.FieldListString(FieldMapList) + _
                         " FROM  " + MasterDataLayer.TableName + ", " + DetailDataLayer.TableName + " " + _
                         " WHERE " + m_DetailForeignKeyFieldName + " = " + m_MasterKeyFieldName
            AddAditionalFilter(NSelectSQL, AditionalFilter)
            Return NSelectSQL
        End Function
    End Class

End Namespace
