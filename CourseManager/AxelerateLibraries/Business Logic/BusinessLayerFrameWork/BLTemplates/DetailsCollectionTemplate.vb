Imports Microsoft.VisualBasic
Namespace BLCore.Templates

    <Serializable()> _
    Public Class DetailsCollectionTemplate( _
        Of tCollectionObject As {DetailsCollectionTemplate(Of tCollectionObject, tMasterObject, tDetailObject), New}, _
        tMasterObject As {AutoNumericIDTemplate(Of tMasterObject), New}, _
        tDetailObject As {DetailBussinessTemplate(Of tMasterObject, tDetailObject), New})
        Inherits BLListBase(Of tCollectionObject, tDetailObject, AutoNumericIDTemplate(Of tDetailObject).BOAutoNumericIDDataKey)

        Public Shared Function GetDetailsInMaster(ByVal MasterObject As tMasterObject) As tCollectionObject
            Dim Criteria As New BusinessListCriteria
            Criteria.DataLayerContextInfo = MasterObject.DataLayerContextInfo
            Dim TmpObject As New tDetailObject
            Dim DataLatyer As DataLayerAbstraction = TmpObject.DataLayer
            Criteria.AddBinaryExpression("MasterID" + DataLatyer.DataLayerFieldSuffix, "MasterID", "=", MasterObject.ID)
            Criteria.DataLayerContextInfo = MasterObject.DataLayerContextInfo
            Dim ToReturn As tCollectionObject = CType(GetCollection(Criteria), tCollectionObject)
            Return ToReturn
        End Function


    End Class
End Namespace