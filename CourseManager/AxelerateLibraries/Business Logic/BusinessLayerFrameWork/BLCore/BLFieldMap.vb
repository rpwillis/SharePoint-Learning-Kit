Imports System.Reflection

Namespace BLCore
    'Clase que contiene la definicion de los mapeos de campos de un objeto de negocios 
    'a ser guardados en la capa de datos.
    <Serializable()> _
    Public Class BLFieldMap

#Region "Private Data"
        'Nombre en la capa de negocios
        Private m_DataLayerFieldName As String

        'Nombre en la capa de datos
        Private m_BusinessLayerFieldName As String

        'Nombre independiente de la capa
        Private m_NeutralFieldName As String

        'Informacion de .Net Framework sobre el campo 
        Private m_Field As FieldInfo

        'Indica si es parte de la llave
        Private m_isKey As Boolean

        'Indica si se utiliza durante la operacion Fetch
        Private m_isFetchField As Boolean

        'Indica si se utiliza durante la operacion Update
        Private m_isUpdateField As Boolean

        'Indica el tipo de autonumérico en caso de que el campo sea autonumeric
        Private m_AutoNumericType As AutoNumericTypeEnum = AutoNumericTypeEnum.NotAutoNumeric

        'Indica si debe obtenerse el autonumerico durante la operacion de Insert
        Private m_AutoNumericRelevant As Boolean

        'Indica un numero que ordena el campo en un orden arbitrario
        Private m_Order As Integer

        'Id
        Private m_FieldMapType As FieldMapTypeEnum

        'Indica si el objeto es una clase de negocios
        Private m_isBusinessClass As Boolean

        'Indica si se utiliza durante la operacion Update
        Private m_isFieldMap As Boolean

        'Indica si se utiliza durante la operacion Update
        Private m_isCachedObject As Boolean

        'Nombre de la propiedad publica asociada con el campo interno
        Private m_PropertyName As String

#End Region

#Region "Public Properties"
        'Retorna el nombre en la base de datos del campo automapeable
        Public ReadOnly Property DLFieldName() As String
            Get
                Return m_DataLayerFieldName
            End Get
        End Property

        'Retorna el nombre en la base de datos del campo automapeable
        Public ReadOnly Property BLFieldName() As String
            Get
                Return m_BusinessLayerFieldName
            End Get
        End Property

        Public ReadOnly Property NeutralFieldName() As String
            Get
                Return m_NeutralFieldName
            End Get
        End Property


        'Retorna la informacion de .Net Framework sobre el campo 
        Public ReadOnly Property Field() As FieldInfo
            Get
                Return m_Field
            End Get
        End Property

        'Retorna true si es parte de la llave
        Public ReadOnly Property isKey() As Boolean
            Get
                Return m_isKey
            End Get
        End Property

        'Retorna true si se utiliza durante la operacion Fetch
        Public ReadOnly Property isFetchField() As Boolean
            Get
                Return m_isFetchField
            End Get
        End Property

        'Retorna true si debe obtenerse el autonumerico durante la operacion de Insert
        Public ReadOnly Property AutoNumericType() As AutoNumericTypeEnum
            Get
                Return m_AutoNumericType
            End Get
        End Property

        'Retorna true si debe obtenerse el autonumerico durante la operacion de Insert
        Public ReadOnly Property isAutoNumericRelevant() As Boolean
            Get
                Return m_AutoNumericRelevant
            End Get
        End Property


        'Retorna true si se utiliza durante la operacion Update
        Public ReadOnly Property isUpdateField() As Boolean
            Get
                Return m_isUpdateField
            End Get
        End Property

        'Retorna true si el campo es una clase de negocios
        Public ReadOnly Property FieldMapType() As FieldMapTypeEnum
            Get
                Return m_FieldMapType
            End Get
        End Property

        'Retorna true si el campo es una clase de negocios
        Public ReadOnly Property Order() As Integer
            Get
                Return m_Order
            End Get
        End Property

        Public ReadOnly Property PropertyName() As String
            Get
                If m_PropertyName = "" Then
                    Return Me.m_NeutralFieldName
                End If
                Return m_PropertyName
            End Get
        End Property



#End Region

#Region "Misc"
        Public Sub New(ByVal DataLayer As BusinessLayerFrameWork.DLCore.DataLayerAbstraction, ByVal FieldMapType As FieldMapTypeEnum, _
            ByVal Field As FieldInfo, ByVal isKey As Boolean, _
            ByVal isFetchField As Boolean, ByVal isUpdateField As Boolean, ByVal Order As Integer, _
            ByVal NAutoNumericType As AutoNumericTypeEnum, ByVal PropertyName As String, _
            ByVal AutoNumericRelevant As Boolean)

            m_NeutralFieldName = Field.Name.Remove(0, DataLayer.ObjectFieldPrefix.Length)
            m_DataLayerFieldName = m_NeutralFieldName + DataLayer.DataLayerFieldSuffix
            m_BusinessLayerFieldName = Field.Name
            m_FieldMapType = FieldMapType
            m_Field = Field
            m_isKey = isKey
            m_isFetchField = isFetchField Or isKey
            m_isUpdateField = isUpdateField 'Or isKey
            m_Order = Order
            m_AutoNumericType = NAutoNumericType
            m_PropertyName = PropertyName
            m_AutoNumericRelevant = AutoNumericRelevant
        End Sub

        Public Sub New(ByVal DataLayer As BusinessLayerFrameWork.DLCore.DataLayerAbstraction, ByVal FieldMapType As FieldMapTypeEnum, _
            ByVal NFieldName As String, ByVal isKey As Boolean, _
            ByVal isFetchField As Boolean, ByVal isUpdateField As Boolean, ByVal Order As Integer, _
            ByVal NAutoNumericType As AutoNumericTypeEnum, ByVal PropertyName As String, _
            ByVal AutoNumericRelevant As Boolean)

            m_Field = Nothing
            m_NeutralFieldName = NFieldName
            m_DataLayerFieldName = m_NeutralFieldName + DataLayer.DataLayerFieldSuffix
            m_BusinessLayerFieldName = DataLayer.ObjectFieldPrefix + m_NeutralFieldName
            m_FieldMapType = FieldMapType
            m_Field = Field
            m_isKey = isKey
            m_isFetchField = isFetchField Or isKey
            m_isUpdateField = isUpdateField 'Or isKey
            m_Order = Order
            m_AutoNumericType = NAutoNumericType
            m_PropertyName = PropertyName
            m_AutoNumericRelevant = AutoNumericRelevant
        End Sub


        'Se redefine el hash code para que se utiliza el identificador único (Criteria).
        Public Overloads Function GetHashCode() As Integer
            Return Me.m_Order.GetHashCode
        End Function

        'Se redefine el metodo Equals de forma que dos objetos se consideran iguales cuando 
        'sus identificadores únicos (Criteria) son iguales.
        Public Overloads Function Equals(ByVal obj As BLFieldMap) As Boolean
            Return (m_DataLayerFieldName = obj.m_DataLayerFieldName) And (m_NeutralFieldName = obj.m_NeutralFieldName) 
        End Function


#End Region

#Region "AutoNumericType Enum"
        Public Enum AutoNumericTypeEnum As Integer
            NotAutoNumeric = 1
            IdentityColumn = 2
            GeneratedColumn = 4
        End Enum
#End Region

#Region "FieldMapType Enum"
        Public Enum FieldMapTypeEnum As Integer
            DataField = 1
            DynamicDataField = 2
            BusinessClassField = 4
            BusinessCollectionField = 8
            CachedObjectField = 16
            Unknown = 32
        End Enum
#End Region

#Region "FieldMapCriteria Class"
        Public Class FieldMapCriteria

            'Criterios de busqueda para un FieldMap
            Private m_FieldMapType As FieldMapTypeEnum
            Private m_isKey As Boolean
            Private m_AutonumericType As AutoNumericTypeEnum
            Private m_isAutonumericRelevant As Boolean
            Private m_isFetch As Boolean
            Private m_isUpdate As Boolean

            'Definen si se usa un criterio o no se usa para una búsqueda
            Private m_UseFieldType As Boolean
            Private m_UseisKey As Boolean
            Private m_UseAutonumericType As Boolean
            Private m_UseisAutonumericRelevant As Boolean
            Private m_UseisFetch As Boolean
            Private m_UseisUpdate As Boolean


            Public Sub New()
                m_FieldMapType = FieldMapTypeEnum.DataField
                m_isKey = False
                m_AutoNumericType = AutoNumericTypeEnum.NotAutoNumeric
                m_isAutonumericRelevant = False
                m_isFetch = False
                m_isUpdate = False

                m_UseFieldType = False
                m_UseisKey = False
                m_UseAutonumericType = False
                m_UseisAutonumericRelevant = False
                m_UseisFetch = False
                m_UseisUpdate = False
            End Sub

            Public Property FieldMapType() As FieldMapTypeEnum
                Get
                    Return m_FieldMapType
                End Get
                Set(ByVal value As FieldMapTypeEnum)
                    m_FieldMapType = value
                    m_UseFieldType = True
                End Set
            End Property

            Public Property isKey() As Boolean
                Get
                    Return m_isKey
                End Get
                Set(ByVal value As Boolean)
                    m_isKey = value
                    m_UseisKey = True
                End Set
            End Property

            Public Property AutonumericType() As AutoNumericTypeEnum
                Get
                    Return m_AutoNumericType
                End Get
                Set(ByVal value As AutoNumericTypeEnum)
                    m_AutoNumericType = value
                    m_UseAutonumericType = True
                End Set
            End Property

            Public Property isAutonumericRelevant() As Boolean
                Get
                    Return m_isAutonumericRelevant
                End Get
                Set(ByVal value As Boolean)
                    m_isAutonumericRelevant = value
                    m_UseisAutonumericRelevant = True
                End Set
            End Property

            Public Property isFetch() As Boolean
                Get
                    Return m_isFetch
                End Get
                Set(ByVal value As Boolean)
                    m_isFetch = value
                    m_UseisFetch = True
                End Set
            End Property

            Public Property isUpdate() As Boolean
                Get
                    Return m_isUpdate
                End Get
                Set(ByVal value As Boolean)
                    m_isUpdate = value
                    m_UseisUpdate = True
                End Set
            End Property

            Public Function Match(ByVal FieldMap As BLFieldMap) As Boolean
                Dim isMatch As Boolean = True

                If isMatch And m_UseisAutonumericRelevant Then
                    isMatch = isMatch And FieldMap.isAutonumericRelevant = isAutonumericRelevant
                End If

                If isMatch And m_UseAutonumericType Then
                    isMatch = isMatch And CBool(FieldMap.AutoNumericType And AutonumericType)
                End If

                If isMatch And m_UseisKey Then
                    isMatch = isMatch And FieldMap.isKey = isKey
                End If

                If isMatch And m_UseisUpdate Then
                    isMatch = isMatch And FieldMap.isUpdateField = isUpdate
                End If

                If isMatch And m_UseisFetch Then
                    isMatch = isMatch And FieldMap.isFetchField = isFetch
                End If

                If isMatch And m_UseFieldType Then
                    isMatch = isMatch And CBool(FieldMap.FieldMapType And FieldMapType)
                End If

                Return isMatch

            End Function




        End Class

#End Region


    End Class
End Namespace