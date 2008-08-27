Imports System.Reflection

Namespace BLCore.Templates

    'Se encarga de implementar las propiedades Get y Set para un objeto de negocios foráneo (que implementa 
    'el template AutoNumericID) con cache
    Public Class BLGUIDForeignPropertyCache( _
        Of tBusinessObject As {GUIDTemplate(Of tBusinessObject), New})
        Inherits BLForeignPropertyCache(Of tBusinessObject, GUIDTemplate(Of tBusinessObject).BOGUIDDataKey)


        Public Shared Shadows Function GetProperty(ByVal Owner As BLBusinessBase, ByRef CacheObject As tBusinessObject, ByVal ForeignGUID As String) As tBusinessObject

            Dim ForeignDataKey As New GUIDTemplate(Of tBusinessObject).BOGUIDDataKey(Owner.DataLayerContextInfo)
            ForeignDataKey.m_GUID = ForeignGUID

            CacheObject = BLForeignPropertyCache(Of tBusinessObject, GUIDTemplate(Of tBusinessObject).BOGUIDDataKey).GetProperty( _
                Owner, CacheObject, ForeignDataKey)
            Return CacheObject
        End Function

        Public Shared Shadows Function SetProperty(ByRef CacheObject As tBusinessObject, ByVal ValueObject As tBusinessObject, _
            ByRef ForeignGUID As String, Optional ByVal CloneObject As Boolean = True) As tBusinessObject

            CacheObject = BLForeignPropertyCache(Of tBusinessObject, GUIDTemplate(Of tBusinessObject).BOGUIDDataKey).SetProperty( _
                CacheObject, ValueObject, CloneObject)

            ForeignGUID = CacheObject.GUID
            Return CacheObject
        End Function

    End Class
End Namespace