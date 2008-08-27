Namespace BLCore
    ''' <summary>
    ''' Interface that contains all the common methods for the different BLCommandBase templated types
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IBLCommandBase
        ''' <summary>
        ''' Gets the Data Source Name
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property DataSourceName() As String

        ''' <summary>
        ''' Gets the Result of the command
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Result() As DataSet

        ''' <summary>
        ''' Gets the Name of the command to be executed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property CommandName() As String

        ''' <summary>
        ''' Gets the parameters for the command to be executed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Parameters() As DLCore.DataLayerParameter()


        ''' <summary>
        ''' Gets the DataLayer associated to the command to be executed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property DataLayer() As DLCore.DataLayerAbstraction

        ''' <summary>
        ''' Gets the boolean isStoredProcedure
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property IsStoredProcedure() As Boolean
    End Interface
End Namespace
