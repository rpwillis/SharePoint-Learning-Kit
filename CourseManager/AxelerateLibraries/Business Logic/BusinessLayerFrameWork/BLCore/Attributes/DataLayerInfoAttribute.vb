Imports System.Reflection
Imports System.Configuration.ConfigurationManager


Namespace BLCore.Attributes

    'Atributo que define si un campo es mapeable
    <AttributeUsage(AttributeTargets.Class)> _
    Public Class DataLayerInfoAttribute
        Inherits Attribute

        'Indica el nombre del archivo XML en el cual se encuentra la información del DataLayer para este objeto
        Private m_XMLFileName As String

        'Ruta completa sobre la cual será buscado el archivo XML
        Private m_XMLFilePath As String = ""

        'Indica si el objeto tiene campos dinamicos cuyo esquema debe ser obtenido de un archivo de datos
        Private m_HasDynamicFields As Boolean = False


        Public Sub New(ByVal NXMLFileName As String, ByVal NHasDynamicFields As Boolean)
            m_XMLFileName = NXMLFileName
            m_XMLFilePath = AppSettings("DataLayerXMLPath") + m_XMLFileName
            m_HasDynamicFields = NHasDynamicFields

        End Sub

        Public ReadOnly Property XMLFileName() As String
            Get
                Return m_XMLFileName
            End Get
        End Property

        Public ReadOnly Property HasDynamicFields() As Boolean
            Get
                Return m_HasDynamicFields
            End Get
        End Property

#Region "Attribute Checking"


        Private Shared AttributeType As Type = GetType(DataLayerInfoAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal NType As Type) As Boolean
            Return Attribute.IsDefined(NType, AttributeType)
        End Function

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function GetAttribute(ByVal NType As Type) As DataLayerInfoAttribute
            Return CType(Attribute.GetCustomAttribute(NType, AttributeType), DataLayerInfoAttribute)
        End Function


#End Region

#Region "XML File Loading"

        'Indica si un archivo XML de registro de información de DataLayer ya ha sido cargado a memoria
        Private Shared m_XMLLoaded As New Hashtable

        Public Sub RegisterDataLayerInfo()
            SyncLock m_XMLLoaded
                If Not m_XMLLoaded.Contains(m_XMLFilePath) Then
                    DataLayerCache.RegisterDataLayerInfo(m_XMLFilePath)
                    m_XMLLoaded(m_XMLFilePath) = True
                End If
            End SyncLock
        End Sub

#End Region



    End Class


End Namespace