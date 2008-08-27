Imports System.Reflection
Imports Axelerate.BusinessLayerFrameWork.BLCore
Imports Axelerate.BusinessLayerFrameWork.BLCore.Security

Namespace BLCodeGeneration

    Public Class BLCodeGenerator
        Inherits ModuleGenerator

#Region "Iteration values"
        Private m_ActualTypeIndex As Integer = -1
        Private m_Types() As System.Type

        Public ReadOnly Property ActualType() As Type
            Get
                If m_ActualTypeIndex < m_Types.Length And m_ActualTypeIndex >= 0 Then
                    Return m_Types(m_ActualTypeIndex)
                Else
                    Return Nothing
                End If
            End Get
        End Property


#End Region


        Public Sub New(ByVal NAssembly As System.Reflection.Assembly, ByVal NModuleNumber As Integer)
            MyBase.New(NModuleNumber)
            m_Types = NAssembly.GetTypes() 'coleccion de clases o enums
        End Sub

        Public Overrides Function OnGetNextType(ByRef TypeName As String) As Boolean
            m_ActualTypeIndex = m_ActualTypeIndex + 1
            If m_ActualTypeIndex < m_Types.Length Then
                'Seteo de valores de reemplazo
                Dim SecAttr As SecurityTokenAttribute = SecurityTokenAttribute.GetAttribute(ActualType)
                While SecAttr Is Nothing And (Not ActualType Is Nothing)
                    m_ActualTypeIndex = m_ActualTypeIndex + 1
                    If Not ActualType Is Nothing Then
                        SecAttr = SecurityTokenAttribute.GetAttribute(ActualType)
                    End If
                End While

                If Not ActualType Is Nothing Then
                    TypeName = ActualType.Name
                    Dim TmpInstance As BLBusinessBase = CType(ActualType.Assembly.CreateInstance(ActualType.FullName), BLBusinessBase)
                    Dim DataLayer As DataLayerAbstraction = TmpInstance.DataLayer

                    TagReplacement("ObjectName") = SecAttr.Name
                    TagReplacement("CollectionName") = SecAttr.Description
                    TagReplacement("TableName") = DataLayer.TableName
                    TagReplacement("TableSuffix") = DataLayer.DataLayerFieldSuffix
                    TagReplacement("ObjectNumber") = TypeConsecutive.ToString
                    Return True
                Else
                    TypeName = ""
                    Return False
                End If

            Else
                TypeName = ""
                Return False
            End If
        End Function
        Private Function MatchTagParameters(ByVal Field As System.Reflection.FieldInfo, ByVal TagParameters() As String) As Boolean
            If (BLCore.Attributes.FieldMapAttribute.isDefined(Field)) Then
                Return True
            End If
            Return False

        End Function

        Public Overrides Function OnForEachTagFound(ByVal TypeName As String, ByVal TagName As String, ByVal TagParameters() As String, ByVal SubTemplate As CodeGeneratorTemplate) As String
            Dim ResultString As String = ""
            Dim TypeFilter As String = ""
            If TagParameters.Length > 0 Then
                TypeFilter = TagParameters(0)
            End If
            Select Case TagName
                Case "Field"
                    For Each NField As System.Reflection.FieldInfo In ActualType.GetFields(Reflection.BindingFlags.FlattenHierarchy Or Reflection.BindingFlags.GetField Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
                        If MatchTagParameters(NField, TagParameters) Then
                            Dim InternalFieldName As String = Mid(NField.Name, 3)
                            Dim FieldMapAttr As FieldMapAttribute = FieldMapAttribute.GetAttribute(NField)
                            Dim PropertyName As String = FieldMapAttr.PropertyName
                            If PropertyName = "" Then
                                PropertyName = InternalFieldName
                            End If
                            Dim NPropertyInfo As System.Reflection.PropertyInfo = ActualType.GetProperty(PropertyName, Reflection.BindingFlags.FlattenHierarchy Or Reflection.BindingFlags.GetProperty Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public)
                            Dim SourceField As String = ""
                            Dim RelatedField As String = ""
                            If ForeignObjectPropertyAttribute.isDefined(NPropertyInfo) Then
                                Dim ForeignObjectAttr As ForeignObjectPropertyAttribute = ForeignObjectPropertyAttribute.GetAttribute(NPropertyInfo)
                                SourceField = ForeignObjectAttr.SourcePropertyName
                                RelatedField = ForeignObjectAttr.ForeignPropertyName
                            End If

                            Dim FilteredOut As Boolean = Not PassFilter(NPropertyInfo, NField, TypeFilter)

                            If Not FilteredOut Then
                                TagReplacement("FieldName") = PropertyName
                                TagReplacement("InternalFieldName") = InternalFieldName
                                TagReplacement("FieldType") = TranslateType(NPropertyInfo.PropertyType.Name)
                                TagReplacement("FieldManagedType") = NPropertyInfo.PropertyType.Name
                                TagReplacement("FieldReferenceType") = "T" + NPropertyInfo.PropertyType.Name
                                TagReplacement("SourceFieldName") = SourceField
                                TagReplacement("RelatedFieldName") = RelatedField
                                SubTemplate.StartInstanceGeneration()
                                SubTemplate.ProcessTags(ActualType.Name)
                                SubTemplate.EndInstanceGeneration(False)
                                ResultString = ResultString + SubTemplate.TemplateInstaceString + vbCrLf
                            End If
                        End If
                    Next
            End Select

            Return ResultString

        End Function


        Public Shared Sub WriteTemplateInstance(ByVal ClassTemplate As String, ByVal OutPutPath As String, ByVal NombreClase As String, ByVal NombreColeccion As String, ByVal ModuleNumber As Integer, ByVal ObjectNumber As Integer, ByVal TableName As String, ByVal TableSuffix As String, ByVal NType As Type, ByVal Extension As String)


            Dim ClassTemplateInstance As String = ClassTemplate

            ReplaceForEachField(ClassTemplateInstance, NType, "ForEachFieldStart", "ForEachFieldEnd", "")
            ReplaceForEachField(ClassTemplateInstance, NType, "ForEachFieldStart", "ForEachFieldEnd", "ValueType")
            ReplaceForEachField(ClassTemplateInstance, NType, "ForEachFieldStart", "ForEachFieldEnd", "String")
            ReplaceForEachField(ClassTemplateInstance, NType, "ForEachFieldStart", "ForEachFieldEnd", "ReferenceType")
            ReplaceForEachField(ClassTemplateInstance, NType, "ForEachFieldStart", "ForEachFieldEnd", "DataKeyType")
            ReplaceForEachField(ClassTemplateInstance, NType, "ForEachFieldStart", "ForEachFieldEnd", "NotReferenceValueType")





        End Sub


        Public Shared Sub ReplaceForEachField(ByRef ClassTemplateInstance As String, ByVal NType As Type, ByVal StartTag As String, ByVal EndTag As String, ByVal TypeFilter As String)
            StartTag = "<" + StartTag
            If TypeFilter <> "" Then
                StartTag = StartTag + "(" + TypeFilter + ")"
            End If
            StartTag = StartTag + ">"

            EndTag = "<" + EndTag
            If TypeFilter <> "" Then
                EndTag = EndTag + "(" + TypeFilter + ")"
            End If
            EndTag = EndTag + ">"


            Dim ForEachFieldStartPos As Integer = ClassTemplateInstance.IndexOf(StartTag)
            Dim ForEachFieldEndPos As Integer = ClassTemplateInstance.IndexOf(EndTag)
            While ForEachFieldStartPos <> -1
                Dim ForEachLineTemplate As String = Mid(ClassTemplateInstance, ForEachFieldStartPos + 1, ForEachFieldEndPos - ForEachFieldStartPos + EndTag.Length)
                Dim TmpStr As String = ForEachLineTemplate
                TmpStr = ForEachLineTemplate.Replace(StartTag, "")
                TmpStr = TmpStr.Replace(EndTag, "")
                Dim ForEachLine As String = ""

                For Each NField As System.Reflection.FieldInfo In NType.GetFields(Reflection.BindingFlags.FlattenHierarchy Or Reflection.BindingFlags.GetField Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)

                    If (BLCore.Attributes.FieldMapAttribute.isDefined(NField)) Then
                        Dim InternalFieldName As String = Mid(NField.Name, 3)
                        Dim FieldMapAttr As FieldMapAttribute = FieldMapAttribute.GetAttribute(NField)
                        Dim PropertyName As String = FieldMapAttr.PropertyName
                        If PropertyName = "" Then
                            PropertyName = InternalFieldName
                        End If
                        Dim NPropertyInfo As System.Reflection.PropertyInfo = NType.GetProperty(PropertyName, Reflection.BindingFlags.FlattenHierarchy Or Reflection.BindingFlags.GetProperty Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public)
                        Dim SourceField As String = ""
                        Dim RelatedField As String = ""
                        If ForeignObjectPropertyAttribute.isDefined(NPropertyInfo) Then
                            Dim ForeignObjectAttr As ForeignObjectPropertyAttribute = ForeignObjectPropertyAttribute.GetAttribute(NPropertyInfo)
                            SourceField = ForeignObjectAttr.SourcePropertyName
                            RelatedField = ForeignObjectAttr.ForeignPropertyName
                        End If

                        Dim FilteredOut As Boolean = Not PassFilter(NPropertyInfo, NField, TypeFilter)

                        If Not FilteredOut Then
                            Dim ThisPropertyStr As String = TmpStr.Replace("<FieldName>", PropertyName)
                            ThisPropertyStr = ThisPropertyStr.Replace("<InternalFieldName>", InternalFieldName)
                            ThisPropertyStr = ThisPropertyStr.Replace("<FieldType>", TranslateType(NPropertyInfo.PropertyType.Name))
                            ThisPropertyStr = ThisPropertyStr.Replace("<FieldManagedType>", NPropertyInfo.PropertyType.Name)
                            ThisPropertyStr = ThisPropertyStr.Replace("<FieldReferenceType>", "T" + NPropertyInfo.PropertyType.Name)
                            ThisPropertyStr = ThisPropertyStr.Replace("<SourceFieldName>", SourceField)
                            ThisPropertyStr = ThisPropertyStr.Replace("<RelatedFieldName>", RelatedField)
                            ForEachLine = ForEachLine + ThisPropertyStr + vbCrLf
                        End If
                    End If
                Next

                ClassTemplateInstance = ClassTemplateInstance.Replace(ForEachLineTemplate, ForEachLine)
                ForEachFieldStartPos = ClassTemplateInstance.IndexOf(StartTag)
                ForEachFieldEndPos = ClassTemplateInstance.IndexOf(EndTag)

            End While


        End Sub

        Public Shared Function PassFilter(ByVal NProperty As System.Reflection.PropertyInfo, ByVal NField As System.Reflection.FieldInfo, ByVal TypeFilter As String) As Boolean
            If TypeFilter = "" Or TypeFilter = "Any" Then
                Return True
            ElseIf TypeFilter = "String" Then
                Return NProperty.PropertyType.Name = "String"
            ElseIf TypeFilter = "ValueType" Then
                Return NProperty.PropertyType.Name <> "String"
            ElseIf TypeFilter = "ReferenceType" Then
                Return NProperty.PropertyType.Name <> "String" And NProperty.PropertyType.Name <> "Double" And NProperty.PropertyType.Name <> "Boolean" And NProperty.PropertyType.Name <> "Int32"
            ElseIf TypeFilter = "NotReferenceValueType" Then
                Return NProperty.PropertyType.Name = "Double" Or NProperty.PropertyType.Name = "Boolean" Or NProperty.PropertyType.Name = "Int32"
            ElseIf TypeFilter = "DataKeyType" Then
                Dim FieldMapAttr As FieldMapAttribute = FieldMapAttribute.GetAttribute(NField)
                Return FieldMapAttr.IsKey
            End If
        End Function

        Public Shared Function TranslateType(ByVal TypeName As String) As String
            Select Case TypeName
                Case "String"
                    Return "char *"
                Case "Integer"
                    Return "int"
                Case "Int32"
                    Return "int"
                Case "Double"
                    Return "double"
                Case "Boolean"
                    Return "char"


            End Select

            Return "T" + TypeName + "*"

        End Function



    End Class
End Namespace