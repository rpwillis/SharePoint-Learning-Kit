Imports System.Reflection
Imports BusinessLayerFrameWork.BLCore
Imports Data_Access_Application_Block
Imports BusinessLayerFrameWork.BLCore.Attributes
Imports System.Configuration.ConfigurationManager

Namespace DLCore

    'Clase que todas los objetos de negocios deben heredar, y contiene la informacion 
    'de metadata generada a partir de los atributos de las clases de negocios
    Public Class XMLDataLayer
        Inherits DataLayerAbstraction

#Region "XML Data"
        'Nombre del string
        Private m_XMLName As String = ""

        'Connection String utilizada para conectarse al XML
        Private m_ConnectionString As String = ""
        'Connection String utilizada para conectarse al schema de XML
        'Private m_SchemaConnectionString As String = ""
        Public ReadOnly Property XMLName() As String
            Get
                Return m_XMLName
            End Get
        End Property

        'Connection String utilizada para conectarse al XML
        Public ReadOnly Property ConnectionString() As String
            Get
                If m_ConnectionString = "" Then
                    m_ConnectionString = ConnectionStrings(XMLName).ConnectionString()
                End If
                Return m_ConnectionString
            End Get
        End Property
        'Connection String utilizada para conectarse al schema del XML
        'Public ReadOnly Property SchemaConnectionString() As String
        '   Get
        '       If m_SchemaConnectionString = "" Then
        'Dim SchemaName As String = "Schema_" + XMLName
        '           m_SchemaConnectionString = ConnectionStrings(SchemaName).ConnectionString()
        '      End If
        '     Return m_SchemaConnectionString
        ' End Get
        'End Property
#End Region

#Region "FieldList Mapping"



#End Region

#Region "Uncategorized"

        Public Sub New(ByVal BusinessObjectType As System.Type, ByVal TableName As String, _
            ByVal DataLayerFieldSuffix As String, Optional ByVal XMLName As String = "XMLSoulRenaissance")
            MyBase.New(TableName, DataLayerFieldSuffix, BusinessObjectType, XMLName)
            Init(BusinessObjectType, XMLName)
        End Sub

        Public Sub New(ByVal BusinessObjectTypeName As String, _
            ByVal TableName As String, _
            ByVal DataLayerFieldSuffix As String, _
            Optional ByVal XMLName As String = "XMLSoulRenaissance")
            MyBase.New(TableName, DataLayerFieldSuffix, ReflectionHelper.CreateBusinessType(BusinessObjectTypeName), XMLName)
            Init(ReflectionHelper.CreateBusinessType(BusinessObjectTypeName), XMLName)
        End Sub

        Private Sub Init(ByVal BusinessObjectType As System.Type, Optional ByVal XMLName As String = "XMLSoulRenaissance")
            BeginDataDefinition()
            AddFieldsFromType(BusinessObjectType)
            EndDataDefinition(True)
            m_XMLName = XMLName
        End Sub

#End Region

#Region "DataLayerAbstraction Overrides"

        Public Overrides Function InsertCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase
            Dim InsertCmd As DataLayerXMLInsertCommand = Nothing
            InsertCmd = New DataLayerXMLInsertCommand(Me, BusinessObject.DataLayerContextInfo.DataSourceName)
            InsertCmd.BusinessObject = BusinessObject


            Return InsertCmd
        End Function

        Public Overrides Function UpdateCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase

            Dim UpdateCmd As DataLayerXMLUpdateCommand = Nothing
            UpdateCmd = New DataLayerXMLUpdateCommand(Me, BusinessObject.DataLayerContextInfo.DataSourceName)
            UpdateCmd.BusinessObject = BusinessObject

            Return UpdateCmd
        End Function

        Public Overrides Function DeleteCommand(ByVal BusinessObject As BLBusinessBase) As DataLayerCommandBase

            Dim DeleteCmd As DataLayerXMLDeleteCommand = Nothing
            DeleteCmd = New DataLayerXMLDeleteCommand(Me, BusinessObject.DataLayerContextInfo.DataSourceName)
            DeleteCmd.BusinessObject = BusinessObject

            Return DeleteCmd
        End Function

        Public Overrides Function ReadCommand(ByVal Caller As Object, ByVal Criteria As BLCriteria, _
            Optional ByVal CachedObjects As List(Of BLFieldMap) = Nothing) As DataLayerCommandBase

            Dim Command As DataLayerXMLReadCommand
            If TypeOf Caller Is BLBusinessBase Then
                Dim BOCaller As BLBusinessBase = CType(Caller, BLBusinessBase)
                Command = New DataLayerXMLReadCommand(Me, Criteria, BOCaller.DataLayerContextInfo.DataSourceName)
            Else
                Dim BCCaller As IBLListBase = CType(Caller, IBLListBase)
                Command = New DataLayerXMLReadCommand(Me, Criteria, BCCaller.DataLayerContextInfo.DataSourceName)
            End If



            Return Command
        End Function


#End Region


    End Class

End Namespace

