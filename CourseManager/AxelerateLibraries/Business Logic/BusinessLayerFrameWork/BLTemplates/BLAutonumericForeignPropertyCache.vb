Imports System.Reflection

Namespace BLCore.Templates

    'Se encarga de implementar las propiedades Get y Set para un objeto de negocios foráneo (que implementa 
    'el template AutoNumericID) con cache
    Public Class BLAutoNumericForeignPropertyCache( _
        Of tBusinessObject As {AutoNumericIDTemplate(Of tBusinessObject), New})
        Inherits BLForeignPropertyCache(Of tBusinessObject, AutoNumericIDTemplate(Of tBusinessObject).BOAutoNumericIDDataKey)


        Public Shared Shadows Function GetProperty(ByVal Owner As BLBusinessBase, ByRef CacheObject As tBusinessObject, ByVal ForeignID As Integer) As tBusinessObject

            Dim ForeignDataKey As New AutoNumericIDTemplate(Of tBusinessObject).BOAutoNumericIDDataKey(Owner.DataLayerContextInfo)
            ForeignDataKey.m_ID = ForeignID

            CacheObject = BLForeignPropertyCache(Of tBusinessObject, AutoNumericIDTemplate(Of tBusinessObject).BOAutoNumericIDDataKey).GetProperty( _
                Owner, CacheObject, ForeignDataKey)
            Return CacheObject
        End Function

        Public Shared Shadows Function SetProperty(ByRef CacheObject As tBusinessObject, ByVal ValueObject As tBusinessObject, _
            ByRef ForeignID As Integer, Optional ByVal CloneObject As Boolean = True) As tBusinessObject

            CacheObject = BLForeignPropertyCache(Of tBusinessObject, AutoNumericIDTemplate(Of tBusinessObject).BOAutoNumericIDDataKey).SetProperty( _
                CacheObject, ValueObject, CloneObject)

            ForeignID = CacheObject.ID
            Return CacheObject
        End Function

    End Class
End Namespace