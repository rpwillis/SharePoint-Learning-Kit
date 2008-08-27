Imports System.Reflection

Namespace BLCore.Attributes
    ''' <summary>
    ''' Attribute that defines the marked field as a cache for a property that returns an external (foreign) object 
    ''' that is related to the actual class by a foreign data key that refers to the object in its corresponding table.  
    ''' Used to make the development of foreign object properties faster and make them more robust.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field)> _
    Public Class CachedForeignObjectAttribute
        Inherits Attribute

        'Name of the property that is able to assign and return the cached object
        Private m_PropertyName As String = ""

        'Name of the first key field 
        Private m_ExternalKeyName1 As String = ""

        'Name of the second key field = ""
        Private m_ExternalKeyName2 As String

        'Name of the third key field = ""
        Private m_ExternalKeyName3 As String

        'Name of the first key field  
        Private m_InternalKeyName1 As String = ""

        'Name of the second key field = ""
        Private m_InternalKeyName2 As String

        'Name of the third key field = ""
        Private m_InternalKeyName3 As String

        'Indicates if the load of this object should be done on demand or not
        Private m_LoadType As CachedObjectLoadType

        'SelectSQL = ""
        Private m_SelectSQL As String = ""

        'Reference to a new object of the type that describes the attribute
        Private m_BusinessObject As BLBusinessBase

        Private m_PropertyType As Type

        Public Sub New(ByVal PropertyName As String, ByVal PropertyType As Type, _
            ByVal InternalKeyName As String, ByVal ExternalKeyName As String, _
            Optional ByVal InternalKeyName2 As String = "", Optional ByVal ExternalKeyName2 As String = "", _
            Optional ByVal InternalKeyName3 As String = "", Optional ByVal ExternalKeyName3 As String = "", _
            Optional ByVal LoadType As CachedObjectLoadType = CachedObjectLoadType.OnDemand)

            m_PropertyName = PropertyName
            m_ExternalKeyName1 = ExternalKeyName
            m_ExternalKeyName2 = ExternalKeyName2
            m_ExternalKeyName3 = ExternalKeyName3

            m_InternalKeyName1 = InternalKeyName
            m_InternalKeyName2 = InternalKeyName2
            m_InternalKeyName3 = InternalKeyName3

            m_LoadType = LoadType
            m_PropertyType = PropertyType

            m_BusinessObject = NewBusinessObjectInstance()
        End Sub

        Public ReadOnly Property PropertyName() As String
            Get
                Return m_PropertyName
            End Get
        End Property

        Public ReadOnly Property ExternalKeyName1() As String
            Get
                Return m_ExternalKeyName1
            End Get
        End Property

        Public ReadOnly Property ExternalKeyName2() As String
            Get
                Return m_ExternalKeyName2
            End Get
        End Property

        Public ReadOnly Property ExternalKeyName3() As String
            Get
                Return m_ExternalKeyName3
            End Get
        End Property

        Public ReadOnly Property InternalKeyName1() As String
            Get
                Return m_InternalKeyName1
            End Get
        End Property

        Public ReadOnly Property InternalKeyName2() As String
            Get
                Return m_InternalKeyName2
            End Get
        End Property

        Public ReadOnly Property InternalKeyName3() As String
            Get
                Return m_InternalKeyName3
            End Get
        End Property

        Public ReadOnly Property LoadType() As CachedObjectLoadType
            Get
                Return m_LoadType
            End Get
        End Property

        Public ReadOnly Property BusinessObject() As BLBusinessBase
            Get
                Return m_BusinessObject
            End Get
        End Property

        Public Function NewBusinessObjectInstance(Optional ByVal DataLayerContextInfo As DLCore.DataLayerContextInfo = Nothing) As BLBusinessBase
            Dim Args() As Object = {DataLayerContextInfo}
            Return CType(m_PropertyType.InvokeMember("NewObject", _
                               BindingFlags.InvokeMethod Or BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic Or Reflection.BindingFlags.Default Or BindingFlags.FlattenHierarchy, _
                               Nothing, Nothing, Args), BLBusinessBase)
        End Function

        Public Sub SetPropertyValue(ByVal ParentObject As BLBusinessBase, ByVal CachedObject As BLBusinessBase)
            Dim ParentType As Type = ParentObject.GetType
            Dim Field As PropertyInfo
            Field = ParentType.GetProperty(PropertyName)
            Field.SetValue(ParentObject, CachedObject, Nothing)
        End Sub

#Region "CachedObjectLoadType"
        Public Enum CachedObjectLoadType As Integer
            OnDemand = 0
            OnCreation = 1
        End Enum
#End Region

#Region "Attribute Checking"

        'Type of the Attribute
        Private Shared AttributeType As Type = GetType(CachedForeignObjectAttribute)

        'Indicates if the field has the AutoFieldAttribute defined
        Public Overloads Shared Function isDefined(ByVal Field As FieldInfo) As Boolean
            Return Attribute.IsDefined(Field, AttributeType)
        End Function

        'Obtains the CachedForeignObjectAttribute
        Public Overloads Shared Function GetAttribute(ByVal Field As FieldInfo) As CachedForeignObjectAttribute
            Return CType(Attribute.GetCustomAttribute(Field, AttributeType), CachedForeignObjectAttribute)
        End Function


#End Region

    End Class
End Namespace