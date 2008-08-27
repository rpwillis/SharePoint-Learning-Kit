Imports Microsoft.VisualBasic

Namespace BLCore.Templates

    <Serializable()> _
    Public Class MNGUIDDetailsCollectionTemplate(Of tCollectionObject As {MNGUIDDetailsCollectionTemplate(Of tCollectionObject, tRelationObject, tMasterObject, tDetailObject), New}, _
        tRelationObject As {MNGUIDRelationBusinessTemplate(Of tRelationObject, tMasterObject, tDetailObject), New}, _
        tMasterObject As {GUIDTemplate(Of tMasterObject), New}, _
        tDetailObject As {GUIDTemplate(Of tDetailObject), New})
        Inherits BLListBase(Of tCollectionObject, tDetailObject, GUIDTemplate(Of tDetailObject).BOGUIDDataKey)

#Region "Shared Methods"

        Public Shared Function GetDetailsInMaster(ByVal MasterObject As tMasterObject) As tCollectionObject
            Dim Criteria As New BusinessListCriteria
            Criteria.Filter = New DetailsInMasterFilter(MasterObject)
            Criteria.DataLayerContextInfo = MasterObject.DataLayerContextInfo
            Return CType(GetCollection(Criteria), tCollectionObject)
        End Function

        Public Shared Function GetDetailsOutOfMaster(ByVal MasterObject As tMasterObject) As tCollectionObject
            Dim Criteria As New BusinessListCriteria
            Criteria.Filter = New DetailsOutOfMasterFilter(MasterObject)
            Criteria.DataLayerContextInfo = MasterObject.DataLayerContextInfo
            Return CType(GetCollection(Criteria), tCollectionObject)
        End Function
#End Region

#Region "SQL Dependent Code"
        <Serializable()> _
        Private Class DetailsInMasterFilter
            Inherits DataLayerFilterBase

            Private m_Master As tMasterObject

            Public Sub New(ByVal Master As tMasterObject)
                m_Master = Master
            End Sub

            Public Overrides Function SelectCommandText(ByVal DataLayer As DataLayerAbstraction, ByVal FieldMapList As BLFieldMapList, ByVal AditionalFilter As String, ByRef ParameterList As List(Of DataLayerParameter)) As String
                Dim MasterDataLayer As SQLDataLayer = CType((New tMasterObject).DataLayer, SQLDataLayer)
                Dim DetailDataLayer As SQLDataLayer = CType((New tDetailObject).DataLayer, SQLDataLayer)
                Dim RelationDataLayer As SQLDataLayer = CType((New tRelationObject).DataLayer, SQLDataLayer)

                Dim NSelectSQL As String
                NSelectSQL = "SELECT " + DetailDataLayer.FieldListString(FieldMapList) + _
                             " FROM  " + RelationDataLayer.TableName + ", " + DetailDataLayer.TableName + " " + _
                             " WHERE DetailGUID" + RelationDataLayer.DataLayerFieldSuffix + " = GUID" + DetailDataLayer.DataLayerFieldSuffix + _
                             " AND MasterGUID" + RelationDataLayer.DataLayerFieldSuffix + " = '" + m_Master.GUID + "'"
                AddAditionalFilter(NSelectSQL, AditionalFilter)
                Return NSelectSQL
            End Function
        End Class

        <Serializable()> _
        Private Class DetailsOutOfMasterFilter
            Inherits DataLayerFilterBase

            Private m_Master As tMasterObject

            Public Sub New(ByVal Master As tMasterObject)
                m_Master = Master
            End Sub

            Public Overrides Function SelectCommandText(ByVal DataLayer As DataLayerAbstraction, ByVal FieldMapList As BLFieldMapList, ByVal AditionalFilter As String, ByRef ParameterList As List(Of DataLayerParameter)) As String
                Dim MasterDataLayer As SQLDataLayer = CType((New tMasterObject).DataLayer, SQLDataLayer)
                Dim DetailDataLayer As SQLDataLayer = CType((New tDetailObject).DataLayer, SQLDataLayer)
                Dim RelationDataLayer As SQLDataLayer = CType((New tRelationObject).DataLayer, SQLDataLayer)

                Dim NSelectSQL As String
                NSelectSQL = "SELECT " + DetailDataLayer.FieldListString(FieldMapList, DetailDataLayer.TableName) + " " + _
                             " FROM  " + DetailDataLayer.TableName + " LEFT OUTER JOIN " + _
                             " (SELECT " + DetailDataLayer.FieldListString(FieldMapList, DetailDataLayer.TableName + "_1") + " " + _
                             " FROM " + DetailDataLayer.TableName + " AS " + DetailDataLayer.TableName + "_1 INNER JOIN " + _
                             RelationDataLayer.TableName + " ON " + DetailDataLayer.TableName + "_1.GUID" + DetailDataLayer.DataLayerFieldSuffix + " = " + RelationDataLayer.TableName + ".DetailGUID" + RelationDataLayer.DataLayerFieldSuffix + " " + _
                             " WHERE (" + RelationDataLayer.TableName + ".MasterGUID" + RelationDataLayer.DataLayerFieldSuffix + " = '" + m_Master.GUID + "') AS DetailsInMaster ON " + DetailDataLayer.TableName + ".GUID" + DetailDataLayer.DataLayerFieldSuffix + " = DetailsInMaster.GUID" + DetailDataLayer.DataLayerFieldSuffix + " " + _
                             " WHERE (DetailsInMaster.GUID" + DetailDataLayer.DataLayerFieldSuffix + " IS NULL)"

                AddAditionalFilter(NSelectSQL, AditionalFilter)
                Return NSelectSQL
            End Function
        End Class

#End Region

    End Class
End Namespace