Public Class ModuleGenerator
    Private m_TypeTemplates As New Collection
    Private m_GlobalTemplates As New Collection
    Private m_ModuleNumber As Integer
    'Private m_OutPutPath As String
    Private m_ActualTypeName As String = ""
    Private m_TypeConsecutive As Integer = 0
    Private m_TagReplacements As New Hashtable

    Public Event TagFound(ByVal TypeName As String, ByVal TagName As String, ByRef TagReplacement As String)
    Public Event ForEachTagFound(ByVal TypeName As String, ByVal TagName As String, ByVal TagParameters() As String, ByVal SubTemplate As CodeGeneratorTemplate, ByRef TagReplacement As String)




#Region "Constructor"
    Public Sub New(ByVal NModuleNumber As Integer)
        m_ModuleNumber = NModuleNumber
        TagReplacement("ModuleNumber") = ModuleNumber.ToString
        TagReplacement("UnitNumber") = "0"
    End Sub
#End Region

#Region "Public Properties and methods"
    Public ReadOnly Property ModuleNumber() As Integer
        Get
            Return m_ModuleNumber
        End Get
    End Property

    Public ReadOnly Property ActualTypeName() As String
        Get
            Return m_ActualTypeName
        End Get
    End Property

    Public Sub AddTempalte(ByVal Template As CodeGeneratorTemplate)
        If Template.Type = CodeGeneratorTemplate.CodeGeneratorTemplateType.ClassTempalte Then
            m_TypeTemplates.Add(Template, Template.Name)
        Else
            m_GlobalTemplates.Add(Template, Template.Name)
        End If
    End Sub


    Public ReadOnly Property TypeConsecutive() As Integer
        Get
            Return m_TypeConsecutive
        End Get
    End Property

    Public Property TagReplacement(ByVal Tag As String) As String
        Get
            Dim ToReturn As String = CType(m_TagReplacements(Tag), String)
            If Not ToReturn Is Nothing Then
                Return ToReturn
            End If
            Return "***Undefined***"
        End Get
        Set(ByVal value As String)
            If value Is Nothing Then
                value = "***Undefined***"
            End If
            m_TagReplacements(Tag) = value

        End Set
    End Property



#End Region

#Region "Instantiation Process"
    Private Sub StartInstanceGeneration()
        m_ActualTypeName = ""
        m_TypeConsecutive = 0
        For Each Template As CodeGeneratorTemplate In m_GlobalTemplates
            Template.StartInstanceGeneration()
        Next
    End Sub

    Private Sub NextType(ByVal TypeName As String)
        m_TypeConsecutive = m_TypeConsecutive + 1
        For Each Template As CodeGeneratorTemplate In m_GlobalTemplates
            Template.ResetActualTagPosition()
            Template.ProcessTags(TypeName)
        Next

        For Each Template As CodeGeneratorTemplate In m_TypeTemplates
            Template.StartInstanceGeneration()
            Template.ProcessTags(TypeName)
            Template.EndInstanceGeneration(True)
        Next

        m_ActualTypeName = TypeName
    End Sub

    Private Sub EndInstanceGeneration()

        For Each Template As CodeGeneratorTemplate In m_GlobalTemplates
            Template.EndInstanceGeneration(True)
        Next



    End Sub

    Public Sub RaiseTagFoundEvent(ByVal TypeName As String, ByVal TagName As String, ByRef TagValue As String)
        RaiseEvent TagFound(TypeName, TagName, TagValue)
    End Sub

    Public Sub RaiseForEachTagFoundEvent(ByVal TypeName As String, ByVal TagName As String, ByVal TagParameters() As String, ByVal SubTemplate As CodeGeneratorTemplate, ByRef TagValues As String)
        RaiseEvent ForEachTagFound(TypeName, TagName, TagParameters, SubTemplate, TagValues)
    End Sub

    Public Sub GenerateCode()
        StartInstanceGeneration()
        Dim TypeName As String = ""
        While OnGetNextType(TypeName)
            NextType(TypeName)
        End While
        EndInstanceGeneration()
    End Sub


#End Region

#Region "ModuleGenerator Overridables"

    Public Overridable Function OnForEachTagFound(ByVal TypeName As String, ByVal TagName As String, ByVal TagParameters() As String, ByVal SubTemplate As CodeGeneratorTemplate) As String
        Return ""
    End Function


    Public Overridable Function OnTagFound(ByVal TypeName As String, ByVal TagName As String) As String
        Return TagReplacement(TagName)
    End Function

    Public Overridable Function OnGetNextType(ByRef TypeName As String) As Boolean
        TypeName = ""
        Return True
    End Function

#End Region

    Private Sub ModuleGenerator_ForEachTagFound(ByVal TypeName As String, ByVal TagName As String, ByVal TagParameters() As String, ByVal SubTemplate As CodeGeneratorTemplate, ByRef TagReplacement As String) Handles Me.ForEachTagFound
        TagReplacement = OnForEachTagFound(TypeName, TagName, TagParameters, SubTemplate)
    End Sub

    Private Sub ModuleGenerator_TagFound(ByVal TypeName As String, ByVal TagName As String, ByRef TagReplacement As String) Handles Me.TagFound
        TagReplacement = OnTagFound(TypeName, TagName)
    End Sub
End Class
