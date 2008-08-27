Imports System.Workflow.ComponentModel
Imports System.Reflection

''' <summary>
''' This command extracts distinct values of a field (or a field Collection) in a collection and returns them in a DataSet
''' </summary>
''' <remarks></remarks>
Public Class BLValueListCommand
    Inherits BLBusinessActivity
#Region "Private Object Data"

    ''' <summary>
    ''' Type of collection on which the command is going to be performed
    ''' </summary>
    ''' <remarks></remarks>
    Private m_CollectionType As Type = Nothing

    ''' <summary>
    ''' Criteria that will filter the value list command
    ''' </summary>
    ''' <remarks></remarks>
    Private m_Criteria As BLCriteria = Nothing

    ''' <summary>
    ''' If the command returns data in the form of a table (or resultset), this data will be stored here
    ''' </summary>
    ''' <remarks></remarks>
    Private m_Result As DataSet

    ''' <summary>
    ''' List of fields from which the distinct values are going to be taken from
    ''' </summary>
    ''' <remarks></remarks>
    Private m_FieldNames() As String


#End Region
#Region "Constructors"
    Public Sub New(ByVal CollectionType As Type, ByVal FieldNames() As String, ByVal Criteria As BLCriteria)
        m_CollectionType = CollectionType
        m_Criteria = New BLCriteria(Criteria)
        m_FieldNames = FieldNames
    End Sub
#End Region

#Region "DataPortal Overrides"
    'Uses the information of the DataLayer to read an object
    Protected Overrides Sub DataPortal_Execute()

        Dim NewCollection As IBLListBase = CType(System.Activator.CreateInstance(m_CollectionType), IBLListBase)
        NewCollection.DataLayerContextInfo = m_Criteria.DataLayerContextInfo

        Dim ValueListCommand As DataLayerCommandBase = NewCollection.DataLayer.ValueListCommand(NewCollection, m_FieldNames, m_Criteria)
        Try
            ValueListCommand.Execute()
            Dim NDataSet As New DataSet()
            Dim NDataTable As DataTable = CreateEmptyDataTable(ValueListCommand)
            NDataSet.Tables.Add(NDataTable)
            Dim DataResponse As Boolean = False
            'and loop through it to create the child objects
            While ValueListCommand.NextRecord
                DataResponse = True
                Dim NDataRow As DataRow = NDataTable.NewRow
                For i As Integer = 0 To ValueListCommand.FieldCount - 1
                    NDataRow.Item(ValueListCommand.FieldName(i)) = ValueListCommand.ReadData(i)
                Next
                NDataTable.Rows.Add(NDataRow)
            End While

            If DataResponse Then
                m_Result = NDataSet
            End If
        Catch ex As System.Exception
            Throw New System.Exception(BLResources.Exception_ExecuteOperationFailed, ex)
        Finally
            ValueListCommand.Finish()
        End Try

    End Sub

#End Region

#Region "Public Properties and Methods"
    Public Sub Execute()
        DataPortal_Execute()
        'm_Result = DataPortal.Execute(Me).m_Result
    End Sub

    Public ReadOnly Property Result() As DataSet
        Get
            Return m_Result
        End Get
    End Property
#End Region

#Region "Private Properties and Methods"

    ''' <summary>
    ''' Creates an Empty DataSet for the data present inside the command
    ''' </summary>
    ''' <param name="Command">Command containing the data to retrieve</param>
    ''' <returns></returns>        ''' <remarks></remarks>
    Private Function CreateEmptyDataTable(ByVal Command As DataLayerCommandBase) As DataTable
        Dim NDataTable As New DataTable("ResultTable")

        For i As Integer = 0 To Command.FieldCount - 1
            Dim NColumn As DataColumn = New DataColumn(Command.FieldName(i), GetType(Object))
            NDataTable.Columns.Add(NColumn)
        Next
        Return NDataTable
    End Function


#End Region





End Class
