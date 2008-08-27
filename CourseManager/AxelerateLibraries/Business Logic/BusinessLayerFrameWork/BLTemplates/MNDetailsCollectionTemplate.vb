Imports Microsoft.VisualBasic

Namespace BLCore.Templates

    <Serializable()> _
    Public Class MNDetailsCollectionTemplate(Of tCollectionObject As {MNDetailsCollectionTemplate(Of tCollectionObject, tRelationObject, tMasterObject, tDetailObject), New}, _
        tRelationObject As {MNRelationBusinessTemplate(Of tRelationObject, tMasterObject, tDetailObject), New}, _
        tMasterObject As {AutoNumericIDTemplate(Of tMasterObject), New}, _
        tDetailObject As {AutoNumericIDTemplate(Of tDetailObject), New})
        Inherits BLListBase(Of tCollectionObject, tDetailObject, AutoNumericIDTemplate(Of tDetailObject).BOAutoNumericIDDataKey)

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
                             " WHERE DetailID" + RelationDataLayer.DataLayerFieldSuffix + " = ID" + DetailDataLayer.DataLayerFieldSuffix + _
                             " AND MasterID" + RelationDataLayer.DataLayerFieldSuffix + " = " + m_Master.ID.ToString
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
                             RelationDataLayer.TableName + " ON " + DetailDataLayer.TableName + "_1.ID" + DetailDataLayer.DataLayerFieldSuffix + " = " + RelationDataLayer.TableName + ".DetailID" + RelationDataLayer.DataLayerFieldSuffix + " " + _
                             " WHERE (" + RelationDataLayer.TableName + ".MasterID" + RelationDataLayer.DataLayerFieldSuffix + " = " + m_Master.ID.ToString + ")) AS DetailsInMaster ON " + DetailDataLayer.TableName + ".ID" + DetailDataLayer.DataLayerFieldSuffix + " = DetailsInMaster.ID" + DetailDataLayer.DataLayerFieldSuffix + " " + _
                             " WHERE (DetailsInMaster.ID" + DetailDataLayer.DataLayerFieldSuffix + " IS NULL)"

                AddAditionalFilter(NSelectSQL, AditionalFilter)
                Return NSelectSQL
            End Function
        End Class

#End Region

    End Class
End Namespace