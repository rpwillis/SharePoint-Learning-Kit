Imports System.Reflection

Namespace BLCore.Attributes

    ''' <summary>
    ''' Defines the relation that a field that belongs to a class has with a field that belongs to a table in the data store.
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Field)> _
    Public Class FieldMapAttribute
        Inherits Attribute

        'Indicates if it's part of the key
        Private m_PropertyName As String
        Private m_isKey As Boolean
        Private m_isFetchField As Boolean
        Private m_isUpdateField As Boolean
        Private m_isAutoNumericRelevant As Boolean
        Private m_AutonumericType As BLFieldMap.AutoNumericTypeEnum


        Public Sub New(ByVal isKey As Boolean, Optional ByVal isFetchField As Boolean = True, Optional ByVal isUpdateField As Boolean = True, _
            Optional ByVal NAutoNumericType As BLFieldMap.AutoNumericTypeEnum = BLFieldMap.AutoNumericTypeEnum.NotAutoNumeric, Optional ByVal PropertyName As String = "", Optional ByVal isAutoNumericRelevant As Boolean = False)
            m_isKey = isKey
            m_isFetchField = isFetchField
            m_isUpdateField = isUpdateField
            m_AutonumericType = NAutoNumericType
            m_PropertyName = PropertyName
            m_isAutoNumericRelevant = IsAutoNumericRelevant
        End Sub

        Public ReadOnly Property IsKey() As Boolean
            Get
                Return m_isKey
            End Get
        End Property

        Public ReadOnly Property IsFetchField() As Boolean
            Get
                Return m_isFetchField
            End Get
        End Property

        Public ReadOnly Property IsUpdateField() As Boolean
            Get
                Return m_isUpdateField
            End Get
        End Property

        Public ReadOnly Property IsAutoNumericRelevant() As Boolean
            Get
                Return m_isAutoNumericRelevant
            End Get
        End Property


        Public ReadOnly Property AutoNumericType() As BLFieldMap.AutoNumericTypeEnum
            Get
                Return m_AutonumericType
            End Get
        End Property

        Public ReadOnly Property PropertyName() As String
            Get
                Return m_PropertyName
            End Get
        End Property


#Region "Attribute Checking"

        'Type of the attribute
        Private Shared AttributeType As Type = GetType(FieldMapAttribute)

        'Indicates if the field has the AutoFieldMapAttribute defined
        Public Overloads Shared Function isDefined(ByVal Field As FieldInfo) As Boolean
            Return Attribute.IsDefined(Field, AttributeType)
        End Function

        'Obtains the AutoFieldMapAttribute
        Public Overloads Shared Function GetAttribute(ByVal Field As FieldInfo) As FieldMapAttribute
            Return CType(Attribute.GetCustomAttribute(Field, AttributeType), FieldMapAttribute)
        End Function


#End Region




    End Class


End Namespace