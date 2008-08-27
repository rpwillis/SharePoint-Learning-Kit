Imports System.Workflow.ComponentModel
Imports System.Reflection

Public Class BLCountCommand
    Inherits BLBusinessActivity
#Region "Private Object Data"
    Private m_CollectionType As Type = Nothing
    Private m_Criteria As BLCriteria = Nothing
    Private m_CountResult As Integer = -1

#End Region
#Region "Constructors"
    Public Sub New(ByVal CollectionType As Type, ByVal Criteria As BLCriteria)
        m_CollectionType = CollectionType
        m_Criteria = New BLCriteria(Criteria)
        m_Criteria.IsCount = True
    End Sub
#End Region

#Region "DataPortal Overrides"
    'Utiliza la infromación del DataLayer para leer un objeto
    Protected Overrides Sub DataPortal_Execute()
        Dim NewCollection As IBLListBase = CType(System.Activator.CreateInstance(m_CollectionType), IBLListBase)
        NewCollection.DataLayerContextInfo = m_Criteria.DataLayerContextInfo
        Dim CountCommand As DataLayerCommandBase = NewCollection.DataLayer.ReadCommand(NewCollection, m_Criteria)
        Try
            CountCommand.Execute()
            CountCommand.NextRecord()
            m_CountResult = CType(CountCommand.ReadData(0), Integer)
        Catch ex As Exception
            Throw ex
        Finally
            CountCommand.Finish()
        End Try


    End Sub

#End Region
#Region "Public Properties and Methods"
    Public Sub Execute()
        DataPortal_Execute()
        'm_CountResult = DataPortal.Execute(Me).m_CountResult
    End Sub

    Public ReadOnly Property Count() As Integer
        Get
            Return m_CountResult
        End Get
    End Property
#End Region





End Class
