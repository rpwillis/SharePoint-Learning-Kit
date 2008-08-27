Imports System.Reflection

'Cached object that allows to create and manage the DataLayers of the busienss objects
Public Class DataLayerCache

    'DataSet that contains the metadata of the business objects
    Private Shared m_DSDataLayers As New DSDataLayerMetaData
    Private Shared m_DataLayers As New Hashtable

    ''' <summary>
    ''' Returns the DataLayer type associated to the business object
    ''' </summary>
    ''' <param name="BusinessType"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Candidate for optimization </remarks>
    Private Shared ReadOnly Property DefaultDataLayerType(ByVal BusinessType As Type) As Type
        Get
            Try
                Dim BusinessObjectTypeName As String = BusinessType.FullName
                'Looks for the business object by type to obtain the name
                Dim BusinessObjects() As DSDataLayerMetaData.BusinessObjectsRow = _
                    CType(m_DSDataLayers.BusinessObjects.Select("TypeName = '" + BusinessObjectTypeName + "'"), DSDataLayerMetaData.BusinessObjectsRow())

                'Looks for the default parameters of the business objects
                Dim BOParameters() As DSDataLayerMetaData.BusinessObjects_DataLayersRow = _
                    CType(m_DSDataLayers.BusinessObjects_DataLayers.Select("isDefaultDataLayer AND BusinessObjectName = '" + BusinessObjects(0).Name + "'"), DSDataLayerMetaData.BusinessObjects_DataLayersRow())

                'Looks the DataLayer by type
                Dim DataLayers() As DSDataLayerMetaData.DataLayersRow = _
                    CType(m_DSDataLayers.DataLayers.Select("Name = '" + BOParameters(0).DataLayerName + "'"), DSDataLayerMetaData.DataLayersRow())

                Return ReflectionHelper.CreateBusinessType(DataLayers(0).TypeName)
            Catch ex As System.Exception
                Return GetType(DLCore.SQLDataLayer)
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Returns the initialization parameters fo a DataLayer associated to a bussines object
    ''' </summary>
    ''' <param name="BusinessType"></param>
    ''' <param name="DataLayerType"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Candidate for optimization </remarks>
    Private Shared ReadOnly Property DataLayerParameters(ByVal BusinessType As Type, ByVal DataLayerType As Type) As DSDataLayerMetaData.BusinessObjects_DataLayersRow
        Get
            Try
                Dim BusinessObjectTypeName As String = BusinessType.FullName
                Dim DataLayerTypeName As String = DataLayerType.FullName
                'Looks for the business object by type to obtain the name
                Dim BusinessObjects() As DSDataLayerMetaData.BusinessObjectsRow = _
                    CType(m_DSDataLayers.BusinessObjects.Select("TypeName = '" + BusinessObjectTypeName + "'"), DSDataLayerMetaData.BusinessObjectsRow())

                'Obtains the default name of theDataLayer type
                Dim DataLayers() As DSDataLayerMetaData.DataLayersRow = _
                    CType(m_DSDataLayers.DataLayers.Select("TypeName = '" + DataLayerTypeName + "'"), DSDataLayerMetaData.DataLayersRow())

                'Looks for the default parameters for the business object
                Dim BOParameters() As DSDataLayerMetaData.BusinessObjects_DataLayersRow = _
                    CType(m_DSDataLayers.BusinessObjects_DataLayers.Select("DataLayerName = '" + DataLayers(0).Name + "' AND BusinessObjectName = '" + BusinessObjects(0).Name + "'"), DSDataLayerMetaData.BusinessObjects_DataLayersRow())

                Return BOParameters(0)

            Catch ex As Exception
                Dim toReturn As DSDataLayerMetaData.BusinessObjects_DataLayersRow
                toReturn = m_DSDataLayers.BusinessObjects_DataLayers.NewBusinessObjects_DataLayersRow()
                toReturn.InitializationString = ""
                Return toReturn
            End Try

        End Get
    End Property

    Private Shared Sub OverrideParameter(ByRef ParameterList() As DataTypes.Pair(Of String, String), ByVal ParameterName As String, ByRef ParameterValue As String)
        If ParameterValue = "" Then
            ParameterValue = DataTypes.Pair(Of String, String).FindSecond(ParameterList, ParameterName)
        Else
            Dim Pair As DataTypes.Pair(Of String, String)
            Pair = DataTypes.Pair(Of String, String).FindPair(ParameterList, ParameterName)
            If Not Pair Is Nothing Then
                Pair.Second = ParameterValue
            Else
                Pair = New DataTypes.Pair(Of String, String)(ParameterName, ParameterValue)
                ReDim Preserve ParameterList(ParameterList.Length)
                ParameterList(ParameterList.Length - 1) = Pair
            End If
        End If

    End Sub


    'Returns the appropiate DataLayer, of the DataLayerType. If DataLayerType  
    'is not specified retunrs the default DataLayer object
    Public Shared ReadOnly Property DataLayer(ByVal DataLayerContextInfo As DataLayerContextInfo) As DataLayerAbstraction
        Get
            SyncLock GetType(DataLayerCache)
                If DataLayerContextInfo.DataLayerType Is Nothing Then
                    DataLayerContextInfo.DataLayerType = DefaultDataLayerType(DataLayerContextInfo.BusinessType)
                End If

                Dim Parameters As DSDataLayerMetaData.BusinessObjects_DataLayersRow
                Parameters = DataLayerParameters(DataLayerContextInfo.BusinessType, DataLayerContextInfo.DataLayerType)
                Dim InitializationParameterList() As DataTypes.Pair(Of String, String) = DataLayerAbstraction.ParseInitializationString(Parameters.InitializationString)

                OverrideParameter(InitializationParameterList, "DataSourceName", DataLayerContextInfo.DataSourceName)
                OverrideParameter(InitializationParameterList, "TableName", DataLayerContextInfo.TableName)
                OverrideParameter(InitializationParameterList, "ClassName", DataLayerContextInfo.BusinessType.FullName)

                If DataLayerContextInfo.IsCacheable Then
                    If Not m_DataLayers.Contains(DataLayerContextInfo) Then
                        'Creates new DataLayer, its added to the cache and returns it
                        Dim ArgList() As Object = {InitializationParameterList}
                        Dim toReturn As DataLayerAbstraction = CType(System.Activator.CreateInstance(DataLayerContextInfo.DataLayerType, ArgList), DataLayerAbstraction)
                        m_DataLayers.Add(DataLayerContextInfo, toReturn)
                        Return toReturn
                    End If
                    'Returns the existing DataLayer
                    Return CType(m_DataLayers(DataLayerContextInfo), DataLayerAbstraction)
                Else
                    'Creates new DataLayer and returns it
                    Dim ArgList() As Object = {InitializationParameterList}
                    Dim toReturn As DataLayerAbstraction = CType(System.Activator.CreateInstance(DataLayerContextInfo.DataLayerType, ArgList), DataLayerAbstraction)
                    Return toReturn
                End If
            End SyncLock
        End Get
    End Property

    Public Shared Sub Clear()
        SyncLock GetType(DataLayerCache)
            m_DataLayers.Clear()
        End SyncLock
    End Sub



    Public Shared Sub RegisterDataLayerInfo(ByVal XMLFileName As String)
        SyncLock GetType(DataLayerCache)
            Try
                Dim DS As New DSDataLayerMetaData
                Dim Stream As New IO.FileStream(XMLFileName, IO.FileMode.Open)
                DS.ReadXml(Stream)
                Stream.Close()
                m_DSDataLayers.DataLayers.Merge(DS.DataLayers)
                m_DSDataLayers.BusinessObjects.Merge(DS.BusinessObjects, False)
                m_DSDataLayers.BusinessObjects_DataLayers.Merge(DS.BusinessObjects_DataLayers, False)
            Catch ex As Exception

            End Try
        End SyncLock
    End Sub

End Class
