Imports System.Workflow.ComponentModel
Imports System.Configuration

Namespace BLCore
    ''' <summary>
    ''' Implements the base class for the execution of a command in the Data Store.  A command will be defined as any custom 
    ''' data operation that is more complicated than a retrieval, insertion, update or deletion of records in a single table.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class BLBusinessActivity
        Inherits Activity


#Region "Private Object Data"



#End Region

#Region "Constructors"
        Public Sub New()
            CreateSecurityOperationsHashTable()
        End Sub
#End Region

#Region "DataPortal Overrides"
        'Uses the information of the DataLayer to read an object
        Protected Overridable Sub DataPortal_Execute()
            BLExecuteCommand()
        End Sub

#End Region

#Region "DataLayer Access"
        'Method overridable, his code will be executed by the activity
        'This method represents the Execute of a activity of the workflow
        Public Overridable Sub BLExecuteCommand()


        End Sub

        'Method overridable, his code will be executed by the test engine
        'This method verifies that the Execute of the activity(BLExecuteCommand)
        'achieves his objective
        Public Overridable Sub Test()

        End Sub

#End Region

#Region "Public Properties and Methods"
        'the workflow activity has a execute method
        'Public Sub Execute()
        '    m_Result = DataPortal.Execute(Me).Result
        'End Sub
        Protected Overrides Function Execute(ByVal executionContext As ActivityExecutionContext) As ActivityExecutionStatus
            Try
                DataPortal_Execute()
                Return ActivityExecutionStatus.Closed

            Catch ex As Exception
                Throw ex
            End Try
        End Function



#End Region

#Region "Security Properties and Methods"
        Private m_SecurityOperations As Hashtable = Nothing

        Private Sub CreateSecurityOperationsHashTable()
            m_SecurityOperations = New Hashtable()
            If (Me.GetType().GetCustomAttributes(GetType(SecurableAttribute), False).Length > 0) Then
                Dim BLSecurityConfigFile As String = ConfigurationManager.AppSettings("BLSecurityConfigFile")
                If BLSecurityConfigFile IsNot Nothing And BLSecurityConfigFile <> "" Then
                    Dim doc As Xml.XmlDocument
                    doc = New Xml.XmlDocument()
                    doc.Load(BLSecurityConfigFile)
                    Dim FoundNodes As System.Xml.XmlNodeList = doc.GetElementsByTagName("DEFAULTSECURITYOPERATIONS")
                    If FoundNodes.Count > 0 Then
                        For Each dfNode As Xml.XmlNode In FoundNodes.Item(0).ChildNodes
                            If dfNode.Name.ToUpper() = "OPERATION" Then
                                m_SecurityOperations(dfNode.Attributes("Name").Value.ToUpper()) = Type.GetType(dfNode.Attributes("Type").Value.ToUpper(), True, True)
                            End If
                        Next
                    End If
                    Dim ClassSecurity As Xml.XmlElement = doc.GetElementById(Me.GetType().FullName)
                    If ClassSecurity IsNot Nothing Then
                        For Each dfNode As Xml.XmlNode In ClassSecurity.ChildNodes
                            If dfNode.Name.ToUpper() = "OPERATION" Then
                                m_SecurityOperations(dfNode.Attributes("Name").Value.ToUpper()) = Type.GetType(dfNode.Attributes("Type").Value.ToUpper(), True, True)
                            End If
                        Next
                    End If
                End If
            End If
        End Sub

        Public Function GetActivityExecutionOperation() As clsBLSecurityOperation
            Dim Operation As clsBLSecurityOperation = Nothing
            If m_SecurityOperations IsNot Nothing AndAlso m_SecurityOperations.ContainsKey("ACTIVITYEXECUTION") Then
                Operation = DirectCast(System.Activator.CreateInstance(DirectCast(m_SecurityOperations("ACTIVITYEXECUTION"), Type)), clsBLSecurityOperation)
            Else
                Return New clsActivityExecutionOperation()
            End If
            Return Operation
        End Function

        Public Sub DemandAccess()
            GetActivityExecutionOperation().DemandAccess()
        End Sub

#End Region


    End Class
End Namespace