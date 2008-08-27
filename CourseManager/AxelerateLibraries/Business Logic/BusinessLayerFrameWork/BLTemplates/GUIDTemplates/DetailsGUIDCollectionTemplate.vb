Imports Microsoft.VisualBasic
Namespace BLCore.Templates

    <Serializable()> _
    Public Class DetailsGUIDCollectionTemplate( _
        Of tCollectionObject As {DetailsGUIDCollectionTemplate(Of tCollectionObject, tMasterObject, tDetailObject), New}, _
        tMasterObject As {GUIDTemplate(Of tMasterObject), New}, _
        tDetailObject As {DetailGUIDBussinessTemplate(Of tMasterObject, tDetailObject), New})
        Inherits BLListBase(Of tCollectionObject, tDetailObject, GUIDTemplate(Of tDetailObject).BOGUIDDataKey)

        Public Shared Function GetDetailsInMaster(ByVal MasterObject As tMasterObject) As tCollectionObject
            Dim Criteria As New BusinessListCriteria
            Criteria.DataLayerContextInfo = MasterObject.DataLayerContextInfo
            Dim TmpObject As New tDetailObject
            Dim DataLatyer As DataLayerAbstraction = TmpObject.DataLayer
            Criteria.AddBinaryExpression("MasterGUID" + DataLatyer.DataLayerFieldSuffix, "MasterGUID", "=", MasterObject.GUID)
            Criteria.DataLayerContextInfo = MasterObject.DataLayerContextInfo
            Dim ToReturn As tCollectionObject = CType(GetCollection(Criteria), tCollectionObject)
            Return ToReturn
        End Function


    End Class
End Namespace