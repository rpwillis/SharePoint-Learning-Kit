Imports System.Reflection
Imports System.Runtime.Serialization

Namespace BLCore
    'Objeto generico que sirve para definir la llave única de un objeto de negocios
    <Serializable()> _
    Public Class BLDataKey

        'Tipo de objeto de negocios al que pertenece esta llave
        Private m_BusinessType As Type

        Protected m_DataLayerContext As DataLayerContextInfo

        'True si llave está vacía
        Public Overridable ReadOnly Property isEmpty() As Boolean
            Get
                Return Equals(Me.NewDataKey)
            End Get
        End Property

        'Agrega los datos actuales de la llave al filtro criteria.
        Public Sub AddToCriteria(ByVal Criteria As BLCriteria)


            Dim Sufix As String = GetDataLayerFieldSuffix(m_BusinessType)
            Dim Fields() As FieldInfo
            Dim Field As FieldInfo
            Dim CurrentType As Type = Me.GetType()
            Dim AttributeType As Type = GetType(Attributes.DataKeyAttribute)
            Dim DynamicAttributeType As Type = GetType(Attributes.DynamicDataKeyAttribute)
            Dim BaseType As Type = GetType(BLDataKey)

            Do
                Fields = CurrentType.GetFields(BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public)

                For Each Field In Fields
                    If Attribute.IsDefined(Field, AttributeType) Then
                        Dim DataKeyAttrib As Attributes.DataKeyAttribute = CType(Attribute.GetCustomAttribute(Field, AttributeType), Attributes.DataKeyAttribute)
                        Criteria.AddBinaryExpression(DataKeyAttrib.PropertyName + Sufix, DataKeyAttrib.PropertyName, "=", Field.GetValue(Me))
                    End If

                    If Attribute.IsDefined(Field, DynamicAttributeType) Then
                        Dim DynamicDataKeyAttrib As Attributes.DynamicDataKeyAttribute = CType(Attribute.GetCustomAttribute(Field, DynamicAttributeType), Attributes.DynamicDataKeyAttribute)
                        Dim List As ArrayList = CType(Field.GetValue(Me), ArrayList)
                        For Each Pair As BLCore.DataTypes.Pair(Of String, Object) In List
                            Criteria.AddBinaryExpression(Pair.First, Pair.First, "=", Pair.Second)
                        Next
                    End If
                Next
                CurrentType = CurrentType.BaseType
            Loop Until CurrentType Is BaseType
        End Sub

        Public Overrides Function ToString() As String

            Dim DatakeyStr As String = ""
            Dim Sufix As String = GetDataLayerFieldSuffix(m_BusinessType)
            Dim Fields() As FieldInfo
            Dim Field As FieldInfo
            Dim CurrentType As Type = Me.GetType()
            Dim AttributeType As Type = GetType(Attributes.DataKeyAttribute)
            Dim DynamicAttributeType As Type = GetType(Attributes.DynamicDataKeyAttribute)
            Dim BaseType As Type = GetType(BLDataKey)

            Do
                Fields = CurrentType.GetFields(BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public)

                For Each Field In Fields
                    If Attribute.IsDefined(Field, AttributeType) Then
                        Dim DataKeyAttrib As Attributes.DataKeyAttribute = CType(Attribute.GetCustomAttribute(Field, AttributeType), Attributes.DataKeyAttribute)
                        'Criteria.AddBinaryExpression(DataKeyAttrib.PropertyName + Sufix, DataKeyAttrib.PropertyName, "=", Field.GetValue(Me))
                        If (DatakeyStr <> "") Then
                            DatakeyStr += "&"
                        End If
                        DatakeyStr += DataKeyAttrib.PropertyName + "=" + Field.GetValue(Me).ToString()
                    End If

                    If Attribute.IsDefined(Field, DynamicAttributeType) Then
                        Dim DynamicDataKeyAttrib As Attributes.DynamicDataKeyAttribute = CType(Attribute.GetCustomAttribute(Field, DynamicAttributeType), Attributes.DynamicDataKeyAttribute)
                        Dim List As ArrayList = CType(Field.GetValue(Me), ArrayList)
                        For Each Pair As BLCore.DataTypes.Pair(Of String, Object) In List
                            'Criteria.AddBinaryExpression(Pair.First, Pair.First, "=", Pair.Second)
                            If (DatakeyStr <> "") Then
                                DatakeyStr += "&"
                            End If
                            DatakeyStr += Pair.First + "=" + Pair.Second.ToString()
                        Next
                    End If
                Next
                CurrentType = CurrentType.BaseType
            Loop Until CurrentType Is BaseType

            If DatakeyStr <> "" Then
                Return DatakeyStr
            Else
                Return MyBase.ToString()
            End If
        End Function

        Public Overloads Function Equals(ByVal DataKey As BLDataKey) As Boolean
            If DataKey Is Nothing Then
                Return False
            End If

            Dim Fields() As FieldInfo
            Dim Field As FieldInfo
            Dim CurrentType As Type = Me.GetType()
            Dim AttributeType As Type = GetType(Attributes.DataKeyAttribute)
            Dim BaseType As Type = GetType(BLDataKey)
            Dim EqualsResult As Boolean = True

            If CurrentType Is DataKey.GetType Then
                Do
                    Fields = CurrentType.GetFields(BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public)

                    For Each Field In Fields
                        If EqualsResult AndAlso Attribute.IsDefined(Field, AttributeType) Then
                            EqualsResult = Field.GetValue(Me).Equals(Field.GetValue(DataKey))
                        End If
                    Next
                    CurrentType = CurrentType.BaseType
                Loop Until CurrentType Is BaseType Or EqualsResult = False

                Return EqualsResult
            End If
            Return False
        End Function

        Public Overridable ReadOnly Property NewDataKey() As BLDataKey
            Get
                Return New BLDataKey(m_BusinessType, m_DataLayerContext)
            End Get
        End Property

        Public Sub New(ByVal BusinessType As Type, Optional ByVal DataLayerContext As DataLayerContextInfo = Nothing)
            m_BusinessType = BusinessType
            m_DataLayerContext = DataLayerContext
        End Sub


#Region "Misc"
        Private Function GetDataLayerFieldSuffix(ByVal BusinessType As System.Type) As String
            Dim DataLayer As DLCore.DataLayerAbstraction
            'Dim MyAssembly As Assembly = BusinessType.Assembly
            Dim BusinessObjectInstance As BLBusinessBase
            'BusinessObjectInstance = CType(MyAssembly.CreateInstance(MyAssembly.GetName.Name + "." + BusinessType.Name), BLBusinessBase)
            BusinessObjectInstance = CType(System.Activator.CreateInstance(BusinessType), BLBusinessBase)
            If Not m_DataLayerContext Is Nothing Then
                BusinessObjectInstance.DataLayerContextInfo = m_DataLayerContext
            End If
            DataLayer = BusinessObjectInstance.DataLayer
            Return DataLayer.DataLayerFieldSuffix
        End Function
#End Region


    End Class

End Namespace
