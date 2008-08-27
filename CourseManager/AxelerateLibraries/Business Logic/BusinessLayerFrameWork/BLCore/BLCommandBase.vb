Namespace BLCore
    ''' <summary>
    ''' Implements the base class for the execution of a command in the Data Store.  A command will be defined as any custom 
    ''' data operation that is more complicated than a retrieval, insertion, update or deletion of records in a single table.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class BLCommandBase(Of DataLayerType As DLCore.DataLayerAbstraction)
        Inherits BLBusinessActivity
        Implements IBLCommandBase


#Region "Private Object Data"

        ''' <summary>
        ''' If the command returns data in the form of a table (or resultset), this data will be stored here
        ''' </summary>
        ''' <remarks></remarks>
        Protected m_Result As DataSet

        ''' <summary>
        ''' Name of the command that will be executed and directly interact with the Data Store
        ''' </summary>
        ''' <remarks></remarks>
        Private m_CommandName As String

        ''' <summary>
        ''' Parameters that will be passed to the command
        ''' </summary>
        ''' <remarks></remarks>
        Protected m_Parameters() As DLCore.DataLayerParameter

        ''' <summary>
        ''' Data Source where the command will be performed
        ''' </summary>
        ''' <remarks></remarks>
        Private m_DataSourceName As String

        ''' <summary>
        ''' DataLayer that will be used to perform the command on the data store
        ''' </summary>
        ''' <remarks></remarks>
        Private m_DataLayer As DLCore.DataLayerAbstraction

        ''' <summary>
        ''' Boolean that will be used to specify wich command type will be used (Text, Stored Procedure)
        ''' </summary>
        ''' <remarks></remarks>
        Private m_isStoredProcedure As Boolean = True

#End Region

#Region "Constructors"
        Public Sub New(ByVal pCommandName As String, ByVal pParameters() As DLCore.DataLayerParameter, ByVal pDataSourceName As String)
            m_CommandName = pCommandName
            m_Parameters = pParameters
            m_DataSourceName = pDataSourceName
        End Sub
        Public Sub New(ByVal pCommandName As String, ByVal pParameters() As DLCore.DataLayerParameter, ByVal pDataSourceName As String, ByVal pIsStoredProcedure As Boolean)
            m_CommandName = pCommandName
            m_Parameters = pParameters
            m_DataSourceName = pDataSourceName
            m_isStoredProcedure = pIsStoredProcedure
        End Sub
#End Region

#Region "DataPortal Overrides"
        'Uses the information of the DataLayer for reading the object
        Protected Overrides Sub DataPortal_Execute()
            SecurityCheck("Execute")
            Dim Options As New TransactionOptions
            Options.IsolationLevel = Transactions.IsolationLevel.ReadCommitted
            Using scope As TransactionScope = New TransactionScope(TransactionScopeOption.RequiresNew, Options)
                BLExecuteCommand()
                scope.Complete()
            End Using

        End Sub

#End Region

#Region "DataLayer Access"
        Public Overrides Sub BLExecuteCommand()
            Dim Command As DataLayerCommandBase = DataLayer.ExecutionCommand(Me)
            Try
                Command.Execute()
                Dim NDataSet As New DataSet()
                Dim NDataTable As DataTable = CreateEmptyDataTable(Command)
                NDataSet.Tables.Add(NDataTable)
                Dim DataResponse As Boolean = False
                'and loop through it to create the child objects
                While Command.NextRecord
                    DataResponse = True
                    Dim NDataRow As DataRow = NDataTable.NewRow
                    For i As Integer = 0 To Command.FieldCount - 1
                        NDataRow.Item(Command.FieldName(i)) = Command.ReadData(i)
                    Next
                    NDataTable.Rows.Add(NDataRow)
                End While

                If DataResponse Then
                    m_Result = NDataSet
                End If
            Catch ex As System.Exception
                Throw New System.Exception(BLResources.Exception_ExecuteOperationFailed, ex)
            Finally
                Command.Finish()
            End Try

        End Sub

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

        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="criteria")> _
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")> _
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")> _
        Private Sub DataPortal_Create(ByVal criteria As Object)
            Throw New NotSupportedException(BLResources.NotSupportedException_Create)
        End Sub

        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="criteria")> _
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")> _
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")> _
        Private Sub DataPortal_Fetch(ByVal criteria As Object)
            Throw New NotSupportedException(BLResources.NotSupportedException_Fetch)
        End Sub

        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")> _
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")> _
        Private Sub DataPortal_Update()
            Throw New NotSupportedException(BLResources.NotSupportedException_Update)
        End Sub

        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId:="criteria")> _
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")> _
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")> _
        Private Sub DataPortal_Delete(ByVal criteria As Object)
            Throw New NotSupportedException(BLResources.NotSupportedException_Delete)
        End Sub

        ''' <summary>
        ''' Called by the server-side DataPortal prior to calling the 
        ''' requested DataPortal_xyz method.
        ''' </summary>
        ''' <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected Overridable Sub DataPortal_OnDataPortalInvoke( _
          ByVal e As EventArgs)

        End Sub

        ''' <summary>
        ''' Called by the server-side DataPortal after calling the 
        ''' requested DataPortal_xyz method.
        ''' </summary>
        ''' <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected Overridable Sub DataPortal_OnDataPortalInvokeComplete( _
          ByVal e As EventArgs)

        End Sub

        ''' <summary>
        ''' Called by the server-side DataPortal if an exception
        ''' occurs during server-side processing.
        ''' </summary>
        ''' <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        ''' <param name="ex">The Exception thrown during processing.</param>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected Overridable Sub DataPortal_OnDataPortalException( _
          ByVal e As EventArgs, ByVal ex As Exception)

        End Sub
#End Region

#Region "Public Properties and Methods"
        Public Sub Execute()
            DataPortal_Execute()
            'm_Result = DataPortal.Execute(Me).Result
        End Sub

        Public ReadOnly Property DataLayer() As DLCore.DataLayerAbstraction Implements IBLCommandBase.DataLayer
            Get
                If m_DataLayer Is Nothing Then
                    m_DataLayer = DataLayerAbstraction.NewDataLayer(Of DataLayerType)(m_DataSourceName)
                End If
                Return m_DataLayer
            End Get
        End Property
        ''' <summary>
        ''' Gets the Data Source Name
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property DataSourceName() As String Implements IBLCommandBase.DataSourceName
            Get
                Return m_DataSourceName
            End Get
        End Property

        ''' <summary>
        ''' Gets the Result of the command
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Result() As DataSet Implements IBLCommandBase.Result
            Get
                Return m_Result
            End Get
        End Property

        ''' <summary>
        ''' Gets the Name of the command to be executed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CommandName() As String Implements IBLCommandBase.CommandName
            Get
                Return m_CommandName
            End Get
        End Property

        ''' <summary>
        ''' Gets the parameters for the command to be executed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Parameters() As DLCore.DataLayerParameter() Implements IBLCommandBase.Parameters
            Get
                Return m_Parameters
            End Get
        End Property
        ''' <summary>
        ''' Gets the boolean isStoredProcedure
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsStoredProcedure() As Boolean Implements IBLCommandBase.IsStoredProcedure
            Get
                Return m_isStoredProcedure
            End Get
        End Property
#End Region

#Region "Security Properties and Methods"
        Public Overridable Function GetSecurityScope() As BLCore.Security.SecurityScope
            Return New BLCore.Security.SecurityScope("")
        End Function

        Friend Overridable Sub SecurityCheck(ByVal NivelAcceso As String)
            'Dim MyPrincipal As MyGenericPrincipal = CType(System.Threading.Thread.CurrentPrincipal, MyGenericPrincipal)
            'Dim BusinessObjectName As String = Me.GetType.Name
            'Try

            '    Dim DemandString As String = BusinessObjectName + "/" + _
            '        GetSecurityContext().IDSede + "/" + NivelAcceso
            '    Dim MyPermission As Permissions.PrincipalPermission
            '    MyPermission = New Permissions.PrincipalPermission(Nothing, DemandString)
            '    MyPermission.Demand()
            'Catch e As System.Exception
            '    Throw New System.Exception("El usuario no tiene el nivel de acceso adecuado (" + NivelAcceso + ") para realizar la operación sobre " + BusinessObjectName)
            'Finally
            'End Try
        End Sub

#End Region


    End Class
End Namespace