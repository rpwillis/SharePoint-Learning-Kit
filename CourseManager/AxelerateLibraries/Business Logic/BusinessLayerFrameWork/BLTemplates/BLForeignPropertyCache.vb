Imports System.Reflection
Imports Axelerate.BusinessLayerFrameWork.BLCore

Namespace BLCore.Templates

    'Se encarga de implementar las propiedades Get y Set para un objeto de negocios foráneo con cache
    Public Class BLForeignPropertyCache( _
        Of tBusinessObject As {BLBusinessBase(Of tBusinessObject, tDataKey), New}, _
        tDataKey As BLDataKey)


        Public Shared Function GetProperty(ByVal Owner As BLBusinessBase, ByVal CacheObject As tBusinessObject, ByVal DataKey As tDataKey) As tBusinessObject
            Dim RefreshProperty As Boolean = False
            If CacheObject Is Nothing Then
                RefreshProperty = True
            ElseIf Not CacheObject.DataKey.Equals(DataKey) Then
                RefreshProperty = True
            End If

            If RefreshProperty Then
                If Not DataKey.isEmpty Then
                    Dim Args() As Object = {DataKey, Owner.DataLayerContextInfo}
                    DataKey.GetType()
                    CacheObject = CType(GetType(tBusinessObject).InvokeMember("GetObject", _
                                            BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static Or BindingFlags.InvokeMethod Or BindingFlags.Default Or BindingFlags.FlattenHierarchy, _
                                            Nothing, Nothing, Args), tBusinessObject)
                Else
                    CacheObject = New tBusinessObject
                End If
            End If

            Return CacheObject
        End Function

        Public Shared Sub SetForeignKey(ByVal Owner As BLBusinessBase, ByVal CacheObject As tBusinessObject, ByVal CachedFieldName As String)
            Dim CachedField As FieldInfo = Owner.GetType().GetField(CachedFieldName, BindingFlags.GetField Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public)
            If CachedForeignObjectAttribute.isDefined(CachedField) Then
                Dim CacheAttribute As CachedForeignObjectAttribute = CachedForeignObjectAttribute.GetAttribute(CachedField)

                Dim OwnerSuffix As String = Owner.DataLayer.DataLayerFieldSuffix
                Dim ForeignSuffix As String = CacheObject.DataLayer.DataLayerFieldSuffix

                If (CacheAttribute.InternalKeyName1 <> "") Then
                    Dim OwnerPropertyName As String = CacheAttribute.InternalKeyName1.Replace(OwnerSuffix, "")
                    Dim ForeignPropertyName As String = CacheAttribute.ExternalKeyName1.Replace(ForeignSuffix, "")
                    Owner.PropertyValue(OwnerPropertyName) = CacheObject.PropertyValue(ForeignPropertyName)
                End If

                If (CacheAttribute.InternalKeyName2 <> "") Then
                    Dim OwnerPropertyName As String = CacheAttribute.InternalKeyName2.Replace(OwnerSuffix, "")
                    Dim ForeignPropertyName As String = CacheAttribute.ExternalKeyName2.Replace(ForeignSuffix, "")
                    Owner.PropertyValue(OwnerPropertyName) = CacheObject.PropertyValue(ForeignPropertyName)
                End If


                If (CacheAttribute.InternalKeyName3 <> "") Then
                    Dim OwnerPropertyName As String = CacheAttribute.InternalKeyName3.Replace(OwnerSuffix, "")
                    Dim ForeignPropertyName As String = CacheAttribute.ExternalKeyName3.Replace(ForeignSuffix, "")
                    Owner.PropertyValue(OwnerPropertyName) = CacheObject.PropertyValue(ForeignPropertyName)
                End If

            End If

        End Sub

        Public Shared Function GetForeignKey(ByVal Owner As BLBusinessBase, ByVal CachedFieldName As String) As tDataKey

            Dim CachedField As FieldInfo = Owner.GetType().GetField(CachedFieldName, BindingFlags.GetField Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public)
            Dim ForeignTmpObject As tBusinessObject = New tBusinessObject

            If CachedForeignObjectAttribute.isDefined(CachedField) Then
                Dim CacheAttribute As CachedForeignObjectAttribute = CachedForeignObjectAttribute.GetAttribute(CachedField)

                Dim OwnerSuffix As String = Owner.DataLayer.DataLayerFieldSuffix
                Dim ForeignSuffix As String = ForeignTmpObject.DataLayer.DataLayerFieldSuffix

                Dim OwnerPropertyName As String = ""
                Dim ForeignPropertyName As String = ""

                If (CacheAttribute.InternalKeyName1 <> "") Then
                    OwnerPropertyName = CacheAttribute.InternalKeyName1
                    If (OwnerSuffix <> "") Then
                        OwnerPropertyName = OwnerPropertyName.Replace(OwnerSuffix, "")
                    End If
                    ForeignPropertyName = CacheAttribute.ExternalKeyName1
                    If (ForeignSuffix <> "") Then
                        ForeignPropertyName = ForeignPropertyName.Replace(ForeignSuffix, "")
                    End If

                    ForeignTmpObject.PropertyValue(ForeignPropertyName) = Owner.PropertyValue(OwnerPropertyName)

                End If

                If (CacheAttribute.InternalKeyName2 <> "") Then
                    OwnerPropertyName = CacheAttribute.InternalKeyName2
                    If (OwnerSuffix <> "") Then
                        OwnerPropertyName = OwnerPropertyName.Replace(OwnerSuffix, "")
                    End If
                    ForeignPropertyName = CacheAttribute.ExternalKeyName2
                    If (ForeignSuffix <> "") Then
                        ForeignPropertyName = ForeignPropertyName.Replace(ForeignSuffix, "")
                    End If
                    ForeignTmpObject.PropertyValue(ForeignPropertyName) = Owner.PropertyValue(OwnerPropertyName)
                End If

                If (CacheAttribute.InternalKeyName3 <> "") Then
                    OwnerPropertyName = CacheAttribute.InternalKeyName3
                    If (OwnerSuffix <> "") Then
                        OwnerPropertyName = OwnerPropertyName.Replace(OwnerSuffix, "")
                    End If
                    ForeignPropertyName = CacheAttribute.ExternalKeyName3
                    If (ForeignSuffix <> "") Then
                        ForeignPropertyName = ForeignPropertyName.Replace(ForeignSuffix, "")
                    End If
                    ForeignTmpObject.PropertyValue(ForeignPropertyName) = Owner.PropertyValue(OwnerPropertyName)
                End If

            End If

            Return ForeignTmpObject.TypedDataKey


        End Function




        Public Shared Function SetProperty(ByVal CacheObject As tBusinessObject, ByVal ValueObject As tBusinessObject, _
            Optional ByVal CloneObject As Boolean = True) As tBusinessObject
            If Not ValueObject Is Nothing Then
                If CloneObject Then
                    CacheObject = ValueObject.Clone
                Else
                    CacheObject = ValueObject
                End If
            Else
                CacheObject = New tBusinessObject
            End If
            Return CacheObject
        End Function

    End Class
End Namespace
