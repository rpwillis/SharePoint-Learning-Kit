Public Class CodeGeneratorTemplate

#Region "CodeGeneratorTemplateType Enum"
    Public Enum CodeGeneratorTemplateType
        ClassTempalte = 0
        GlobalTempalte = 1
        TemporalTemplate = 2
    End Enum
#End Region

#Region "Private Data"
    Private m_ModuleGenerator As ModuleGenerator
    Private m_ClassTemplateString As String = ""
    Private m_ClassTemplateInstanceString As String = ""
    Private m_Name As String
    Private m_Type As CodeGeneratorTemplateType
    Private m_ClassTemplateInputFilePath As String
    Private m_ClassTemplateOutputFilePath As String
    Private m_ClassTemplateInputFileName As String
    Private m_ClassTemplateOutputFileName As String



#End Region

#Region "Constructor"
    Public Sub New(ByVal ModuleGenerator As ModuleGenerator, ByVal Name As String, _
        ByVal ClassTemplateInputFilePath As String, ByVal ClassTemplateOutputFilePath As String, _
        ByVal ClassTemplateInputFileName As String, ByVal ClassTemplateOutputFileName As String, _
        ByVal TemplateType As CodeGeneratorTemplateType)
        m_ModuleGenerator = ModuleGenerator
        m_Name = Name
        m_Type = TemplateType
        m_ClassTemplateInputFilePath = ClassTemplateInputFilePath
        m_ClassTemplateOutputFilePath = ClassTemplateOutputFilePath
        m_ClassTemplateInputFileName = ClassTemplateInputFileName
        m_ClassTemplateOutputFileName = ClassTemplateOutputFileName
    End Sub

    Public Sub New(ByVal ModuleGenerator As ModuleGenerator, ByVal TemplateString As String)

        m_ModuleGenerator = ModuleGenerator
        m_Name = ""
        m_Type = CodeGeneratorTemplateType.TemporalTemplate
        m_ClassTemplateInputFilePath = ""
        m_ClassTemplateOutputFilePath = ""
        m_ClassTemplateInputFileName = ""
        m_ClassTemplateOutputFileName = ""
        m_ClassTemplateString = TemplateString
    End Sub
#End Region

#Region "Public Properties and Methods"
    Public ReadOnly Property Type() As CodeGeneratorTemplateType
        Get
            Return m_Type
        End Get
    End Property

    Public ReadOnly Property Name() As String
        Get
            Return m_Name
        End Get
    End Property

    Public ReadOnly Property TemplateString() As String
        Get
            If m_ClassTemplateString = "" Then
                m_ClassTemplateString = GetTemplateFileString(m_ClassTemplateInputFilePath + m_ClassTemplateInputFileName)
            End If
            Return m_ClassTemplateString

        End Get
    End Property

    Public ReadOnly Property TemplateInstaceString() As String
        Get
            Return m_ClassTemplateInstanceString

        End Get
    End Property



#End Region

#Region "Private Properties and Methods"
    Private Function GetTemplateFileString(ByVal FileName As String) As String
        Dim TemplateReader As New System.IO.StreamReader(FileName)
        Dim TemplateString As String = ReadText(TemplateReader)
        TemplateReader.Close()
        Return TemplateString
    End Function

    Private Function ReadText(ByVal Stream As System.IO.StreamReader) As String
        Dim line As String = ""
        Dim text As String = ""
        Dim lineCount As Integer = 0
        ' Read and display the lines from the file until the end 
        ' of the file is reached.
        Do
            line = Stream.ReadLine()
            text = text + line + vbCrLf
            lineCount = lineCount + 1
        Loop Until line Is Nothing

        Return text
    End Function
#End Region

#Region "Instantiotion Process"
    Private m_ActualTypeName As String = ""
    Private m_ActualTagPosition As Integer = 0
    Private m_TemplateLastTagPosition As Integer = 0
    Private m_TemplateActualTagPosition As Integer = 0

    Protected Property ActualTagPosition() As Integer
        Get
            Return m_ActualTagPosition
        End Get
        Set(ByVal value As Integer)
            m_ActualTagPosition = value
        End Set
    End Property

    Protected Property TemplateActualTagPosition() As Integer
        Get
            Return m_TemplateActualTagPosition
        End Get
        Set(ByVal value As Integer)
            m_TemplateLastTagPosition = m_TemplateActualTagPosition
            m_TemplateActualTagPosition = value
        End Set
    End Property

    Protected ReadOnly Property TemplateLastTagPosition() As Integer
        Get
            Return m_TemplateLastTagPosition
        End Get
    End Property




    Public Sub StartInstanceGeneration()
        m_ClassTemplateInstanceString = ""
        ResetActualTagPosition()
    End Sub

    Public Sub ResetActualTagPosition()
        ActualTagPosition = 0
        TemplateActualTagPosition = 0
    End Sub

    Public Sub ProcessTags(ByVal TypeName As String)
        m_ClassTemplateInstanceString = m_ClassTemplateInstanceString + String.Copy(TemplateString) + vbCrLf
        m_ActualTypeName = TypeName
        While NextTag()
            ProcessActualTag()

        End While
    End Sub

    Public Sub EndInstanceGeneration(ByVal WriteToFile As Boolean)
        If (WriteToFile And m_ClassTemplateOutputFileName <> "" And m_ClassTemplateOutputFilePath <> "" And m_ClassTemplateOutputFileName <> "") Then
            Dim TmpTemplate As CodeGeneratorTemplate = New CodeGeneratorTemplate(m_ModuleGenerator, m_ClassTemplateOutputFileName)
            TmpTemplate.StartInstanceGeneration()
            TmpTemplate.ProcessTags(m_ActualTypeName)
            TmpTemplate.EndInstanceGeneration(False)
            Dim ActualOutputFileName As String = TmpTemplate.TemplateInstaceString
            ActualOutputFileName = ActualOutputFileName.Remove(ActualOutputFileName.Length - 2, 2)

            Dim TemplateWriter As New System.IO.StreamWriter(m_ClassTemplateOutputFilePath + ActualOutputFileName)
            TemplateWriter.Write(TemplateInstaceString)
            TemplateWriter.WriteLine()
            TemplateWriter.Close()
        End If
    End Sub

    Private Function NextTag() As Boolean
        If ActualTagPosition = -1 Then
            Return False
        Else
            ActualTagPosition = NextTagPosition("<!", ActualTagPosition, False)
            Return True
        End If
    End Function

    Private Sub ProcessActualTag()
        If ActualTagPosition <> -1 Then
            Dim TagEndPosition As Integer = NextTagPosition("!>", ActualTagPosition, True)
            Dim FirstTag As String = m_ClassTemplateInstanceString.Substring(ActualTagPosition, TagEndPosition - ActualTagPosition + 2)
            If FirstTag.ToUpper.StartsWith("<!FOREACH") Then
                Dim TagContent As String = ""
                Dim ContentStartPosition As Integer = TagEndPosition + 2
                Dim FirstTagStartPosition As Integer = ActualTagPosition
                Dim SecondTag As String = FirstTag.Replace("Start", "End")
                ActualTagPosition = NextTagPosition(SecondTag, TagEndPosition, True)
                'Busca el inicio del tag de finalización del foreach
                TagEndPosition = NextTagPosition("!>", ActualTagPosition, True)
                'Busca el cierre de la llave de finalización de foreach
                Dim ContentEndPosition As Integer = ActualTagPosition - 1
                Dim ContentTag As String = m_ClassTemplateInstanceString.Substring(ContentStartPosition, ContentEndPosition - ContentStartPosition + 1)
                Dim ReplacementTag As String = ProcessForEachTag(FirstTag, "", ContentTag)
                m_ClassTemplateInstanceString = m_ClassTemplateInstanceString.Remove(FirstTagStartPosition, TagEndPosition - FirstTagStartPosition + 2)
                m_ClassTemplateInstanceString = m_ClassTemplateInstanceString.Insert(FirstTagStartPosition, ReplacementTag)
                ActualTagPosition = FirstTagStartPosition
            ElseIf FirstTag.ToUpper.StartsWith("<!IF(") Then
                Dim FirstTagStartPosition As Integer = ActualTagPosition
                Dim ParenthesisEndPosition As Integer = NextTagPosition(")", ActualTagPosition, True)
                Dim Condition As String = m_ClassTemplateInstanceString.Substring(ActualTagPosition + 5, ParenthesisEndPosition - ActualTagPosition - 5)
                Dim ContentTag As String = ""
                ActualTagPosition = NextTagPosition("!>", ParenthesisEndPosition, True)
                Dim ContentStartPosition As Integer = ActualTagPosition + 2
                ActualTagPosition = NextTagPosition("<!EndIF!>", ActualTagPosition, True)
                ContentTag = m_ClassTemplateInstanceString.Substring(ContentStartPosition, ActualTagPosition - ContentStartPosition)
                Dim ReplacementTag As String = ProcessIfTag(Condition, ContentTag)
                m_ClassTemplateInstanceString = m_ClassTemplateInstanceString.Remove(FirstTagStartPosition, ActualTagPosition - FirstTagStartPosition + 10)
                m_ClassTemplateInstanceString = m_ClassTemplateInstanceString.Insert(FirstTagStartPosition, ReplacementTag)
                ActualTagPosition = FirstTagStartPosition

            Else
                Dim TagReplacement As String = ProcessTag(FirstTag)
                m_ClassTemplateInstanceString = m_ClassTemplateInstanceString.Remove(ActualTagPosition, FirstTag.Length)
                m_ClassTemplateInstanceString = m_ClassTemplateInstanceString.Insert(ActualTagPosition, TagReplacement)
            End If
        End If
    End Sub

    Private Function ProcessTag(ByVal Tag As String) As String
        Dim TagName As String = Tag.Substring(2, Tag.Length - 4)
        Dim TagReplacement As String = ""
        m_ModuleGenerator.RaiseTagFoundEvent(m_ActualTypeName, TagName, TagReplacement)
        Return TagReplacement
    End Function

    Private Function ProcessIFTag(ByVal Condition As String, ByVal ContentTag As String) As String
        Dim ReturnValue As String = ""
        Dim OperatorPosition As Integer = Condition.IndexOf("!=")
        Dim EqualOperator As Boolean = True
        Dim TagName As String = ""
        Dim TagValue As String = ""
        Dim ConditionPassed As Boolean = True
        If OperatorPosition <> -1 Then
            EqualOperator = False
            TagName = Condition.Substring(0, OperatorPosition)
            TagValue = Condition.Substring(OperatorPosition + 2)
        Else
            OperatorPosition = Condition.IndexOf("=")
            If OperatorPosition = -1 Then
                Throw New Exception(BLResources.Exception_ConditionalOperatorNotFound)
            End If
            TagName = Condition.Substring(0, OperatorPosition)
            TagValue = Condition.Substring(OperatorPosition + 1)
        End If

        If EqualOperator Then
            ConditionPassed = m_ModuleGenerator.TagReplacement(TagName) = TagValue
        Else
            ConditionPassed = Not m_ModuleGenerator.TagReplacement(TagName) = TagValue
        End If

        If ConditionPassed Then
            Dim TmpTemplate As New CodeGeneratorTemplate(m_ModuleGenerator, ContentTag)
            TmpTemplate.StartInstanceGeneration()
            TmpTemplate.ProcessTags(m_ActualTypeName)
            TmpTemplate.EndInstanceGeneration(False)
            ReturnValue = TmpTemplate.TemplateInstaceString
        End If
        Return ReturnValue
    End Function

    Private Function ProcessForEachTag(ByVal StartTag As String, ByVal EndTag As String, ByVal TagContent As String) As String
        Dim TagParametersStartPosition As Integer = StartTag.IndexOf("(")
        Dim TagParametersEndPosition As Integer = -1
        Dim Parameters() As String = {}
        Dim TagName As String = ""
        If TagParametersStartPosition <> -1 Then
            TagParametersEndPosition = StartTag.IndexOf(")", TagParametersStartPosition)
            If TagParametersEndPosition <> -1 Then
                Dim ParameterString As String = StartTag.Substring(TagParametersStartPosition + 1, TagParametersEndPosition - TagParametersStartPosition - 1)
                Parameters = ParameterString.Split(",".ToCharArray)
                TagName = StartTag.Substring(9, TagParametersStartPosition - 14)
            Else
                Throw New Exception(BLResources.Exception_ForEachCloseTagNotFound)
            End If
        Else
            TagName = StartTag.Substring(9, StartTag.Length - 16)
        End If

        Dim ReturnValue As String = ""
        Dim TmpTemplate As New CodeGeneratorTemplate(m_ModuleGenerator, TagContent)
        m_ModuleGenerator.RaiseForEachTagFoundEvent(m_ActualTypeName, TagName, Parameters, TmpTemplate, ReturnValue)
        Return ReturnValue
    End Function

    Private Function FindLineNumber(ByVal TagPosition As Integer) As Integer
        Dim LineNumber As Integer = 0
        Dim LastLineFeedPos As Integer = m_ClassTemplateInstanceString.IndexOf(vbLf)
        While LastLineFeedPos <> -1 And LastLineFeedPos < TagPosition
            LineNumber = LineNumber + 1
            LastLineFeedPos = m_ClassTemplateInstanceString.IndexOf(vbLf, LastLineFeedPos + 1)
        End While
        Return LineNumber
    End Function

    Private Function NextTagPosition(ByVal TagID As String, ByVal NActualTagPosition As Integer, ByVal ThrowExceptionIfMissing As Boolean) As Integer

        Dim InstanceTagPosition As Integer = 0
        'TemplateActualTagPosition = m_ClassTemplateString.IndexOf(TagID, TemplateActualTagPosition, System.StringComparison.OrdinalIgnoreCase)
        InstanceTagPosition = m_ClassTemplateInstanceString.IndexOf(TagID, NActualTagPosition, System.StringComparison.OrdinalIgnoreCase)
        If InstanceTagPosition = -1 And ThrowExceptionIfMissing Then
            Throw New Exception(BLResources.Exception_TagNotFound.Replace("[TagName]", TagID))
        End If
        Return InstanceTagPosition
    End Function



#End Region


End Class
