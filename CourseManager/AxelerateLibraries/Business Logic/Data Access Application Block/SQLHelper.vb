
Imports System
Imports System.Threading
Imports System.Collections.Generic
Imports System.Text
Imports System.Net.Mail
Imports System.Resources


    ' The SqlHelper class is intended to encapsulate high performance, scalable best practices for 
    ' common uses of SqlClient.

Public NotInheritable Class SqlHelper

#Region "private utility methods & constructors"

        'Since this class provides only static methods, make the default constructor private to prevent 
        'instances from being created with "new SqlHelper()".
        Private Sub New()
        End Sub 'New

        ' This method is used to attach array of SqlParameters to a SqlCommand.
        ' This method will assign a value of DbNull to any parameter with a direction of
        ' InputOutput and a value of null.  
        ' This behavior will prevent default values from being used, but
        ' this will be the less common case than an intended pure output parameter (derived as InputOutput)
        ' where the user provided no input value.
        ' Parameters:
        ' -command - The command to which the parameters will be added
        ' -commandParameters - an array of SqlParameters tho be added to command
        Private Shared Sub AttachParameters(ByVal command As SqlCommand, ByVal commandParameters() As SqlParameter)
            Dim p As SqlParameter
            For Each p In commandParameters
                'check for derived output value with no value assigned
                If p.Direction = ParameterDirection.InputOutput And p.Value Is Nothing Then
                    p.Value = Nothing
                End If
                command.Parameters.Add(p)
            Next p
        End Sub 'AttachParameters

        ' This method assigns an array of values to an array of SqlParameters.
        ' Parameters:
        ' -commandParameters - array of SqlParameters to be assigned values
        ' -array of objects holding the values to be assigned
        Private Shared Sub AssignParameterValues(ByVal commandParameters() As SqlParameter, ByVal parameterValues() As Object)

            Dim i As Short
            Dim j As Short

            If (commandParameters Is Nothing) And (parameterValues Is Nothing) Then
                'do nothing if we get no data
                Return
            End If

            ' we must have the same number of values as we pave parameters to put them in
        If commandParameters.Length <> parameterValues.Length Then
            Throw New ArgumentException(My.Resources.ErrorMessages.strParamError)
        End If

            'value array
            j = commandParameters.Length - 1
            For i = 0 To j
               commandParameters(i).Value = parameterValues(i)
            Next

        End Sub 'AssignParameterValues

        ' This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        ' to the provided command.
        ' Parameters:
        ' -command - the SqlCommand to be prepared
        ' -connection - a valid SqlConnection, on which to execute this command
        ' -transaction - a valid SqlTransaction, or 'null'
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command
        ' -commandParameters - an array of SqlParameters to be associated with the command or 'null' if no parameters are required
        Private Shared Sub PrepareCommand(ByVal command As SqlCommand, _
                                          ByVal connection As SqlConnection, _
                                          ByVal transaction As SqlTransaction, _
                                          ByVal commandType As CommandType, _
                                          ByVal commandText As String, _
                                          ByVal commandParameters() As SqlParameter)

            'if the provided connection is not open, we will open it
            If connection.State <> ConnectionState.Open Then
                connection.Open()
            End If

            'associate the connection with the command
            command.Connection = connection

            'set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText

            'if we were provided a transaction, assign it.
            If Not (transaction Is Nothing) Then
                command.Transaction = transaction
            End If

            'set the command type
            command.CommandType = commandType

            'attach the command parameters if they are provided
            If Not (commandParameters Is Nothing) Then
                AttachParameters(command, commandParameters)
            End If

            Return
        End Sub 'PrepareCommand

#End Region

#Region "ExecuteNonQuery"

        ' Execute a SqlCommand (that returns no resultset and takes no parameters) against the database specified in 
        ' the connection string. 
        ' e.g.:  
        '  Dim result as Integer =  ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders")
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command
        ' Returns: an int representing the number of rows affected by the command
        Public Overloads Shared Function ExecuteNonQuery(ByVal connectionString As String, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String) As Integer
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteNonQuery(connectionString, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteNonQuery

        ' Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        ' using the provided parameters.
        ' e.g.:  
        ' Dim result as Integer = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command
        ' -commandParameters - an array of SqlParamters used to execute the command
        ' Returns: an int representing the number of rows affected by the command
        Public Overloads Shared Function ExecuteNonQuery(ByVal connectionString As String, _
                                                         ByVal commandType As CommandType, _
                                                         ByVal commandText As String, _
                                                         ByVal ParamArray commandParameters() As SqlParameter) As Integer
            'create & open a SqlConnection, and dispose of it after we are done.
            Dim cn As New SqlConnection(connectionString)
            Try
                cn.Open()

                'call the overload that takes a connection in place of the connection string
                Return ExecuteNonQuery(cn, commandType, commandText, commandParameters)
            Finally
                cn.Dispose()
            End Try
        End Function 'ExecuteNonQuery

        ' Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
        ' the connection string using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        '  Dim result as Integer = ExecuteNonQuery(connString, "PublishOrders", 24, 36)
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection
        ' -spName - the name of the stored procedure
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure
        ' Returns: an int representing the number of rows affected by the command
        Public Overloads Shared Function ExecuteNonQuery(ByVal connectionString As String, _
                                                         ByVal spName As String, _
                                                         ByVal ParamArray parameterValues() As Object) As Integer
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)

                 commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteNonQuery

        ' Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlConnection. 
        ' e.g.:  
        ' Dim result as Integer = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders")
        ' Parameters:
        ' -connection - a valid SqlConnection
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command 
        ' Returns: an int representing the number of rows affected by the command
        Public Overloads Shared Function ExecuteNonQuery(ByVal connection As SqlConnection, _
                                                         ByVal commandType As CommandType, _
                                                         ByVal commandText As String) As Integer
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteNonQuery(connection, commandType, commandText, CType(Nothing, SqlParameter()))

        End Function 'ExecuteNonQuery

        ' Execute a SqlCommand (that returns no resultset) against the specified SqlConnection 
        ' using the provided parameters.
        ' e.g.:  
        '  Dim result as Integer = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: an int representing the number of rows affected by the command 
        Public Overloads Shared Function ExecuteNonQuery(ByVal connection As SqlConnection, _
                                                         ByVal commandType As CommandType, _
                                                         ByVal commandText As String, _
                                                         ByVal ParamArray commandParameters() As SqlParameter) As Integer

            'create a command and prepare it for execution
            Dim cmd As New SqlCommand()
            Dim retval As integer
            
            PrepareCommand(cmd, connection, CType(Nothing, SqlTransaction), commandType, commandText, commandParameters)

            'finally, execute the command.
            retval = cmd.ExecuteNonQuery()

            'detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()
            
            return retval

        End Function 'ExecuteNonQuery

        ' Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection 
        ' using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        '  Dim result as integer = ExecuteNonQuery(conn, "PublishOrders", 24, 36)
        ' Parameters:
        ' -connection - a valid SqlConnection
        ' -spName - the name of the stored procedure 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure 
        ' Returns: an int representing the number of rows affected by the command 
        Public Overloads Shared Function ExecuteNonQuery(ByVal connection As SqlConnection, _
                                                         ByVal spName As String, _
                                                         ByVal ParamArray parameterValues() As Object) As Integer
             Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                 commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName)
            End If

        End Function 'ExecuteNonQuery

        ' Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction.
        ' e.g.:  
        '  Dim result as Integer = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders")
        ' Parameters:
        ' -transaction - a valid SqlTransaction associated with the connection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' Returns: an int representing the number of rows affected by the command 
        Public Overloads Shared Function ExecuteNonQuery(ByVal transaction As SqlTransaction, _
                                                         ByVal commandType As CommandType, _
                                                         ByVal commandText As String) As Integer
            'pass through the call providing null for the set of SqlParameters
                Return ExecuteNonQuery(transaction, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteNonQuery

        ' Execute a SqlCommand (that returns no resultset) against the specified SqlTransaction
        ' using the provided parameters.
        ' e.g.:  
        ' Dim result as Integer = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -transaction - a valid SqlTransaction 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: an int representing the number of rows affected by the command 
        Public Overloads Shared Function ExecuteNonQuery(ByVal transaction As SqlTransaction, _
                                                         ByVal commandType As CommandType, _
                                                         ByVal commandText As String, _
                                                         ByVal ParamArray commandParameters() As SqlParameter) As Integer
            'create a command and prepare it for execution
            Dim cmd As New SqlCommand()
            Dim retval As Integer
            
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters)

            'finally, execute the command.
            retval = cmd.ExecuteNonQuery()
        
            'detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()
            
            return retval
            
        End Function 'ExecuteNonQuery

        ' Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlTransaction 
        ' using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim result As Integer = SqlHelper.ExecuteNonQuery(trans, "PublishOrders", 24, 36)
        ' Parameters:
        ' -transaction - a valid SqlTransaction 
        ' -spName - the name of the stored procedure 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure 
        ' Returns: an int representing the number of rows affected by the command 
        Public Overloads Shared Function ExecuteNonQuery(ByVal transaction As SqlTransaction, _
                                                         ByVal spName As String, _
                                                         ByVal ParamArray parameterValues() As Object) As Integer
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                 commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteNonQuery

#End Region

#Region "ExecuteDataset"

        ' Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        ' the connection string. 
        ' e.g.:  
        ' Dim ds As DataSet = SqlHelper.ExecuteDataset("", commandType.StoredProcedure, "GetOrders")
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteDataset(ByVal connectionString As String, _
                                                        ByVal commandType As CommandType, _
                                                        ByVal commandText As String) As DataSet
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteDataset(connectionString, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteDataset

        ' Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        ' using the provided parameters.
        ' e.g.:  
        ' Dim ds as Dataset = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command
        ' -commandParameters - an array of SqlParamters used to execute the command
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteDataset(ByVal connectionString As String, _
                                                        ByVal commandType As CommandType, _
                                                        ByVal commandText As String, _
                                                        ByVal ParamArray commandParameters() As SqlParameter) As DataSet
            'create & open a SqlConnection, and dispose of it after we are done.
            Dim cn As New SqlConnection(connectionString)
            Try
                cn.Open()

                'call the overload that takes a connection in place of the connection string
                Return ExecuteDataset(cn, commandType, commandText, commandParameters)
            Finally
                cn.Dispose()
            End Try
        End Function 'ExecuteDataset

        ' Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        ' the connection string using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim ds as Dataset= ExecuteDataset(connString, "GetOrders", 24, 36)
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection
        ' -spName - the name of the stored procedure
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteDataset(ByVal connectionString As String, _
                                                        ByVal spName As String, _
                                                        ByVal ParamArray parameterValues() As Object) As DataSet

            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteDataset

        ' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        ' e.g.:  
        ' Dim ds as Dataset = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders")
        ' Parameters:
        ' -connection - a valid SqlConnection
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteDataset(ByVal connection As SqlConnection, _
                                                        ByVal commandType As CommandType, _
                                                        ByVal commandText As String) As DataSet

            'pass through the call providing null for the set of SqlParameters
            Return ExecuteDataset(connection, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteDataset

        ' Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ' using the provided parameters.
        ' e.g.:  
        ' Dim ds as Dataset = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -connection - a valid SqlConnection
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command
        ' -commandParameters - an array of SqlParamters used to execute the command
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteDataset(ByVal connection As SqlConnection, _
                                                        ByVal commandType As CommandType, _
                                                        ByVal commandText As String, _
                                                        ByVal ParamArray commandParameters() As SqlParameter) As DataSet
   
            'create a command and prepare it for execution
            Dim cmd As New SqlCommand()
            Dim ds As New DataSet()
            Dim da As SqlDataAdapter

            PrepareCommand(cmd, connection, CType(Nothing, SqlTransaction), commandType, commandText, commandParameters)

            'create the DataAdapter & DataSet
            da = New SqlDataAdapter(cmd)

            'fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds)
            
            'detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()
           
            'return the dataset
            Return ds

        End Function 'ExecuteDataset

        ' Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ' using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim ds As Dataset = ExecuteDataset(conn, "GetOrders", 24, 36)
        ' Parameters:
        ' -connection - a valid SqlConnection
        ' -spName - the name of the stored procedure
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteDataset(ByVal connection As SqlConnection, _
                                                        ByVal spName As String, _
                                                        ByVal ParamArray parameterValues() As Object) As DataSet
            'Return ExecuteDataset(connection, spName, parameterValues)
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteDataset(connection, CommandType.StoredProcedure, spName)
            End If

        End Function 'ExecuteDataset


        ' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        ' e.g.:  
        ' Dim ds As Dataset = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders")
        ' Parameters
        ' -transaction - a valid SqlTransaction
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteDataset(ByVal transaction As SqlTransaction, _
                                                        ByVal commandType As CommandType, _
                                                        ByVal commandText As String) As DataSet
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteDataset(transaction, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteDataset

        ' Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        ' using the provided parameters.
        ' e.g.:  
        ' Dim ds As Dataset = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24))
        ' Parameters
        ' -transaction - a valid SqlTransaction 
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command
        ' -commandParameters - an array of SqlParamters used to execute the command
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteDataset(ByVal transaction As SqlTransaction, _
                                                        ByVal commandType As CommandType, _
                                                        ByVal commandText As String, _
                                                        ByVal ParamArray commandParameters() As SqlParameter) As DataSet
            'create a command and prepare it for execution
            Dim cmd As New SqlCommand()
            Dim ds As New DataSet()
            Dim da As SqlDataAdapter

            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters)

            'create the DataAdapter & DataSet
            da = New SqlDataAdapter(cmd)

            'fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds)
            
            'detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()
            
            'return the dataset
            Return ds
        End Function 'ExecuteDataset

        ' Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified
        ' SqlTransaction using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim ds As Dataset = ExecuteDataset(trans, "GetOrders", 24, 36)
        ' Parameters:
        ' -transaction - a valid SqlTransaction 
        ' -spName - the name of the stored procedure
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteDataset(ByVal transaction As SqlTransaction, _
                                                        ByVal spName As String, _
                                                        ByVal ParamArray parameterValues() As Object) As DataSet
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteDataset(transaction, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteDataset

#End Region

#Region "ExecuteReader"
        ' this enum is used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that
        ' we can set the appropriate CommandBehavior when calling ExecuteReader()
        Private Enum SqlConnectionOwnership
            'Connection is owned and managed by SqlHelper
            Internal
            'Connection is owned and managed by the caller
            [External]
        End Enum 'SqlConnectionOwnership

        ' Create and prepare a SqlCommand, and call ExecuteReader with the appropriate CommandBehavior.
        ' If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        ' If the caller provided the connection, we want to leave it to them to manage.
        ' Parameters:
        ' -connection - a valid SqlConnection, on which to execute this command 
        ' -transaction - a valid SqlTransaction, or 'null' 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParameters to be associated with the command or 'null' if no parameters are required 
        ' -connectionOwnership - indicates whether the connection parameter was provided by the caller, or created by SqlHelper 
        ' Returns: SqlDataReader containing the results of the command 
        Private Overloads Shared Function ExecuteReader(ByVal connection As SqlConnection, _
                                                        ByVal transaction As SqlTransaction, _
                                                        ByVal commandType As CommandType, _
                                                        ByVal commandText As String, _
                                                        ByVal commandParameters() As SqlParameter, _
                                                        ByVal connectionOwnership As SqlConnectionOwnership) As SqlDataReader
            'create a command and prepare it for execution
            Dim cmd As New SqlCommand()
            'create a reader
            Dim dr As SqlDataReader

            PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters)

            ' call ExecuteReader with the appropriate CommandBehavior
            If connectionOwnership = SqlConnectionOwnership.External Then
                dr = cmd.ExecuteReader()
            Else
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            End If

            'detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()
                        
            Return dr
        End Function 'ExecuteReader

        ' Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        ' the connection string. 
        ' e.g.:  
        ' Dim dr As SqlDataReader = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders")
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' Returns: a SqlDataReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteReader(ByVal connectionString As String, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String) As SqlDataReader
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteReader(connectionString, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteReader

        ' Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        ' using the provided parameters.
        ' e.g.:  
        ' Dim dr As SqlDataReader = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: a SqlDataReader containing the resultset generated by the command 
    Public Overloads Shared Function ExecuteReader(ByVal connectionString As String, _
                                                   ByVal commandType As CommandType, _
                                                   ByVal commandText As String, _
                                                   ByVal ParamArray commandParameters() As SqlParameter) As SqlDataReader
        'this change conflicted with error handling
        ' this is test (Regis) to try to loop at least 2 times before returning an error
        'Dim i As Integer
        'For i = 0 To 1
        'create & open a SqlConnection
        'Dim cn As New SqlConnection(connectionString)
        'cn.Open()

        'Try
        'call the private overload that takes an internally owned connection in place of the connection string
        'Return ExecuteReader(cn, CType(Nothing, SqlTransaction), commandType, commandText, commandParameters, SqlConnectionOwnership.Internal)
        'Catch ex As System.Exception
        'if we fail to return the SqlDatReader, we need to close the connection ourselves
        'cn.Dispose()
        ' rethrow the error 
        'If i = 1 Then
        'Throw ex
        'End If
        'End Try
        ' sleep
        'Thread.Sleep(300)
        'Next i

        'create & open a SqlConnection
        Dim cn As New SqlConnection(connectionString)
        cn.Open()

        Try
            'call the private overload that takes an internally owned connection in place of the connection string
            Return ExecuteReader(cn, CType(Nothing, SqlTransaction), commandType, commandText, commandParameters, SqlConnectionOwnership.Internal)
        Catch ex As System.Exception
            'if we fail to return the SqlDatReader, we need to close the connection ourselves
            cn.Dispose()
            ' rethrow the error 
            Throw ex
        End Try
    End Function 'ExecuteReader

        ' Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        ' the connection string using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim dr As SqlDataReader = ExecuteReader(connString, "GetOrders", 24, 36)
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection 
        ' -spName - the name of the stored procedure 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure 
        ' Returns: a SqlDataReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteReader(ByVal connectionString As String, _
                                                       ByVal spName As String, _
                                                       ByVal ParamArray parameterValues() As Object) As SqlDataReader
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                 commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteReader(connectionString, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteReader

        ' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        ' e.g.:  
        ' Dim dr As SqlDataReader = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders")
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' Returns: a SqlDataReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteReader(ByVal connection As SqlConnection, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String) As SqlDataReader

            Return ExecuteReader(connection, commandType, commandText, CType(Nothing, SqlParameter()))

        End Function 'ExecuteReader

        ' Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ' using the provided parameters.
        ' e.g.:  
        ' Dim dr As SqlDataReader = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: a SqlDataReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteReader(ByVal connection As SqlConnection, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String, _
                                                       ByVal ParamArray commandParameters() As SqlParameter) As SqlDataReader
            'pass through the call to private overload using a null transaction value
            Return ExecuteReader(connection, CType(Nothing, SqlTransaction), commandType, commandText, commandParameters, SqlConnectionOwnership.External)

        End Function 'ExecuteReader

        ' Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ' using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim dr As SqlDataReader = ExecuteReader(conn, "GetOrders", 24, 36)
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -spName - the name of the stored procedure 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure 
        ' Returns: a SqlDataReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteReader(ByVal connection As SqlConnection, _
                                                       ByVal spName As String, _
                                                       ByVal ParamArray parameterValues() As Object) As SqlDataReader
            'pass through the call using a null transaction value
            'Return ExecuteReader(connection, CType(Nothing, SqlTransaction), spName, parameterValues)

            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName)

                AssignParameterValues(commandParameters, parameterValues)

            Return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteReader(connection, CommandType.StoredProcedure, spName)
            End If

        End Function 'ExecuteReader

        ' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        ' e.g.:  
        ' Dim dr As SqlDataReader = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders")
        ' Parameters:
        ' -transaction - a valid SqlTransaction  
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' Returns: a SqlDataReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteReader(ByVal transaction As SqlTransaction, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String) As SqlDataReader
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteReader(transaction, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteReader

        ' Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        ' using the provided parameters.
        ' e.g.:  
        ' Dim dr As SqlDataReader = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -transaction - a valid SqlTransaction 
        ' -commandType - the CommandType (stored procedure, text, etc.)
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: a SqlDataReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteReader(ByVal transaction As SqlTransaction, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String, _
                                                       ByVal ParamArray commandParameters() As SqlParameter) As SqlDataReader
            'pass through to private overload, indicating that the connection is owned by the caller
            Return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External)
        End Function 'ExecuteReader

        ' Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
        ' using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim dr As SqlDataReader = ExecuteReader(trans, "GetOrders", 24, 36)
        ' Parameters:
        ' -transaction - a valid SqlTransaction 
        ' -spName - the name of the stored procedure 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure
        ' Returns: a SqlDataReader containing the resultset generated by the command
        Public Overloads Shared Function ExecuteReader(ByVal transaction As SqlTransaction, _
                                                       ByVal spName As String, _
                                                       ByVal ParamArray parameterValues() As Object) As SqlDataReader
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName)

                AssignParameterValues(commandParameters, parameterValues)

                Return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteReader(transaction, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteReader

#End Region

#Region "ExecuteScalar"

        ' Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        ' the connection string. 
        ' e.g.:  
        ' Dim orderCount As Integer = CInt(ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount"))
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' Returns: an object containing the value in the 1x1 resultset generated by the command
        Public Overloads Shared Function ExecuteScalar(ByVal connectionString As String, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String) As Object
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteScalar(connectionString, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteScalar

        ' Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        ' using the provided parameters.
        ' e.g.:  
        ' Dim orderCount As Integer = Cint(ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24)))
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: an object containing the value in the 1x1 resultset generated by the command 
        Public Overloads Shared Function ExecuteScalar(ByVal connectionString As String, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String, _
                                                       ByVal ParamArray commandParameters() As SqlParameter) As Object
            'create & open a SqlConnection, and dispose of it after we are done.
            Dim cn As New SqlConnection(connectionString)
            Try
                cn.Open()

                'call the overload that takes a connection in place of the connection string
                Return ExecuteScalar(cn, commandType, commandText, commandParameters)
            Finally
                cn.Dispose()
            End Try
        End Function 'ExecuteScalar

        ' Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
        ' the connection string using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim orderCount As Integer = CInt(ExecuteScalar(connString, "GetOrderCount", 24, 36))
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection 
        ' -spName - the name of the stored procedure 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure 
        ' Returns: an object containing the value in the 1x1 resultset generated by the command 
        Public Overloads Shared Function ExecuteScalar(ByVal connectionString As String, _
                                                       ByVal spName As String, _
                                                       ByVal ParamArray parameterValues() As Object) As Object
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteScalar

        ' Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlConnection. 
        ' e.g.:  
        ' Dim orderCount As Integer = CInt(ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount"))
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' Returns: an object containing the value in the 1x1 resultset generated by the command 
        Public Overloads Shared Function ExecuteScalar(ByVal connection As SqlConnection, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String) As Object
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteScalar(connection, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteScalar

        ' Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        ' using the provided parameters.
        ' e.g.:  
        ' Dim orderCount As Integer = CInt(ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24)))
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: an object containing the value in the 1x1 resultset generated by the command 
        Public Overloads Shared Function ExecuteScalar(ByVal connection As SqlConnection, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String, _
                                                       ByVal ParamArray commandParameters() As SqlParameter) As Object
            'create a command and prepare it for execution
            Dim cmd As New SqlCommand()
            Dim retval As object
            
            PrepareCommand(cmd, connection, CType(Nothing, SqlTransaction), commandType, commandText, commandParameters)

            'execute the command & return the results
            retval = cmd.ExecuteScalar()

            'detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()
            
            return retval
            
        End Function 'ExecuteScalar

        ' Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        ' using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim orderCount As Integer = CInt(ExecuteScalar(conn, "GetOrderCount", 24, 36))
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -spName - the name of the stored procedure 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure 
        ' Returns: an object containing the value in the 1x1 resultset generated by the command 
        Public Overloads Shared Function ExecuteScalar(ByVal connection As SqlConnection, _
                                                       ByVal spName As String, _
                                                       ByVal ParamArray parameterValues() As Object) As Object

            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                 commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteScalar(connection, CommandType.StoredProcedure, spName)
            End If

        End Function 'ExecuteScalar

        ' Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction.
        ' e.g.:  
        ' Dim orderCount As Integer  = CInt(ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount"))
        ' Parameters:
        ' -transaction - a valid SqlTransaction 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' Returns: an object containing the value in the 1x1 resultset generated by the command 
        Public Overloads Shared Function ExecuteScalar(ByVal transaction As SqlTransaction, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String) As Object
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteScalar(transaction, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteScalar

        ' Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
        ' using the provided parameters.
        ' e.g.:  
        ' Dim orderCount As Integer = CInt(ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24)))
        ' Parameters:
        ' -transaction - a valid SqlTransaction  
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: an object containing the value in the 1x1 resultset generated by the command 
        Public Overloads Shared Function ExecuteScalar(ByVal transaction As SqlTransaction, _
                                                       ByVal commandType As CommandType, _
                                                       ByVal commandText As String, _
                                                       ByVal ParamArray commandParameters() As SqlParameter) As Object
            'create a command and prepare it for execution
            Dim cmd As New SqlCommand()
            Dim retval As Object
            
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters)

            'execute the command & return the results
            retval = cmd.ExecuteScalar()
            
            'detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()
            
            return retval
            
        End Function 'ExecuteScalar

        ' Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction 
        ' using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim orderCount As Integer = CInt(ExecuteScalar(trans, "GetOrderCount", 24, 36))
        ' Parameters:
        ' -transaction - a valid SqlTransaction 
        ' -spName - the name of the stored procedure 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure 
        ' Returns: an object containing the value in the 1x1 resultset generated by the command 
        Public Overloads Shared Function ExecuteScalar(ByVal transaction As SqlTransaction, _
                                                       ByVal spName As String, _
                                                       ByVal ParamArray parameterValues() As Object) As Object
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                 commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteScalar(transaction, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteScalar

#End Region

#Region "ExecuteXmlReader"

        ' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        ' e.g.:  
        ' Dim r As XmlReader = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders")
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command using "FOR XML AUTO" 
        ' Returns: an XmlReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteXmlReader(ByVal connection As SqlConnection, _
                                                          ByVal commandType As CommandType, _
                                                          ByVal commandText As String) As XmlReader
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteXmlReader(connection, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteXmlReader

        ' Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ' using the provided parameters.
        ' e.g.:  
        ' Dim r As XmlReader = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command using "FOR XML AUTO" 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: an XmlReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteXmlReader(ByVal connection As SqlConnection, _
                                                          ByVal commandType As CommandType, _
                                                          ByVal commandText As String, _
                                                          ByVal ParamArray commandParameters() As SqlParameter) As XmlReader
            'pass through the call using a null transaction value
            'Return ExecuteXmlReader(connection, CType(Nothing, SqlTransaction), commandType, commandText, commandParameters)
             'create a command and prepare it for execution
            Dim cmd As New SqlCommand()
            Dim retval As XmlReader
            
            PrepareCommand(cmd, connection, CType(Nothing, SqlTransaction), commandType, commandText, commandParameters)

            'create the DataAdapter & DataSet
            retval = cmd.ExecuteXmlReader()

            'detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()
            
            return retval
            

        End Function 'ExecuteXmlReader

        ' Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ' using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim r As XmlReader = ExecuteXmlReader(conn, "GetOrders", 24, 36)
        ' Parameters:
        ' -connection - a valid SqlConnection 
        ' -spName - the name of the stored procedure using "FOR XML AUTO" 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure 
        ' Returns: an XmlReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteXmlReader(ByVal connection As SqlConnection, _
                                                          ByVal spName As String, _
                                                          ByVal ParamArray parameterValues() As Object) As XmlReader
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                 commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection.ConnectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteXmlReader


        ' Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction
        ' e.g.:  
        ' Dim r As XmlReader = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders")
        ' Parameters:
        ' -transaction - a valid SqlTransaction
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command using "FOR XML AUTO" 
        ' Returns: an XmlReader containing the resultset generated by the command 
        Public Overloads Shared Function ExecuteXmlReader(ByVal transaction As SqlTransaction, _
                                                          ByVal commandType As CommandType, _
                                                          ByVal commandText As String) As XmlReader
            'pass through the call providing null for the set of SqlParameters
            Return ExecuteXmlReader(transaction, commandType, commandText, CType(Nothing, SqlParameter()))
        End Function 'ExecuteXmlReader

        ' Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        ' using the provided parameters.
        ' e.g.:  
        ' Dim r As XmlReader = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24))
        ' Parameters:
        ' -transaction - a valid SqlTransaction
        ' -commandType - the CommandType (stored procedure, text, etc.) 
        ' -commandText - the stored procedure name or T-SQL command using "FOR XML AUTO" 
        ' -commandParameters - an array of SqlParamters used to execute the command 
        ' Returns: an XmlReader containing the resultset generated by the command
        Public Overloads Shared Function ExecuteXmlReader(ByVal transaction As SqlTransaction, _
                                                          ByVal commandType As CommandType, _
                                                          ByVal commandText As String, _
                                                          ByVal ParamArray commandParameters() As SqlParameter) As XmlReader
            'create a command and prepare it for execution
            Dim cmd As New SqlCommand()
            Dim retval As XmlReader
            
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters)

            'create the DataAdapter & DataSet
            retval = cmd.ExecuteXmlReader()
            
            'detach the SqlParameters from the command object, so they can be used again
            cmd.Parameters.Clear()
            
            return retval
            
        End Function 'ExecuteXmlReader

        ' Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
        ' using the provided parameter values.  This method will discover the parameters for the 
        ' stored procedure, and assign the values based on parameter order.
        ' This method provides no access to output parameters or the stored procedure's return value parameter.
        ' e.g.:  
        ' Dim r As XmlReader = ExecuteXmlReader(trans, "GetOrders", 24, 36)
        ' Parameters:
        ' -transaction - a valid SqlTransaction
        ' -spName - the name of the stored procedure 
        ' -parameterValues - an array of objects to be assigned as the input values of the stored procedure 
        ' Returns: a dataset containing the resultset generated by the command
        Public Overloads Shared Function ExecuteXmlReader(ByVal transaction As SqlTransaction, _
                                                          ByVal spName As String, _
                                                          ByVal ParamArray parameterValues() As Object) As XmlReader
            Dim commandParameters As SqlParameter()

            'if we receive parameter values, we need to figure out where they go
            If Not (parameterValues Is Nothing) And parameterValues.Length > 0 Then
                'pull the parameters for this stored procedure from the parameter cache (or discover them & populate the cache)
                 commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection.ConnectionString, spName)

                'assign the provided values to these parameters based on parameter order
                AssignParameterValues(commandParameters, parameterValues)

                'call the overload that takes an array of SqlParameters
                Return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters)
                'otherwise we can just call the SP without params
            Else
                Return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName)
            End If
        End Function 'ExecuteXmlReader

#End Region

End Class 'SqlHelper

' SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
' ability to discover parameters for stored procedures at run-time.
Public NotInheritable Class SqlHelperParameterCache

#Region "private methods, variables, and constructors"


        'Since this class provides only static methods, make the default constructor private to prevent 
        'instances from being created with "new SqlHelperParameterCache()".
        Private Sub New()
        End Sub 'New 

        Private Shared paramCache As Hashtable = Hashtable.Synchronized(New Hashtable())

        ' resolve at run time the appropriate set of SqlParameters for a stored procedure
        ' Parameters:
        ' - connectionString - a valid connection string for a SqlConnection
        ' - spName - the name of the stored procedure
        ' - includeReturnValueParameter - whether or not to include their return value parameter>
        ' Returns: SqlParameter()
        Private Shared Function DiscoverSpParameterSet(ByVal connectionString As String, _
                                                       ByVal spName As String, _
                                                       ByVal includeReturnValueParameter As Boolean, _
                                                       ByVal ParamArray parameterValues() As Object) As SqlParameter()

            Dim cn As New SqlConnection(connectionString)
            Dim cmd As SqlCommand = New SqlCommand(spName, cn)
            Dim discoveredParameters() As SqlParameter

            Try
                cn.Open()
                cmd.CommandType = CommandType.StoredProcedure
                SqlCommandBuilder.DeriveParameters(cmd)
                If Not includeReturnValueParameter Then
                    cmd.Parameters.RemoveAt(0)
                End If

                discoveredParameters = New SqlParameter(cmd.Parameters.Count - 1) {}
                cmd.Parameters.CopyTo(discoveredParameters, 0)
            Finally
                cmd.Dispose()
                cn.Dispose()

            End Try

            Return discoveredParameters

        End Function 'DiscoverSpParameterSet

        'deep copy of cached SqlParameter array
        Private Shared Function CloneParameters(ByVal originalParameters() As SqlParameter) As SqlParameter()

            Dim i As Short
            Dim j As Short = originalParameters.Length - 1
            Dim clonedParameters(j) As SqlParameter

            For i = 0 To j
                clonedParameters(i) = CType(CType(originalParameters(i), ICloneable).Clone, SqlParameter)
            Next

            Return clonedParameters
        End Function 'CloneParameters



#End Region

#Region "caching functions"

        ' add parameter array to the cache
        ' Parameters
        ' -connectionString - a valid connection string for a SqlConnection 
        ' -commandText - the stored procedure name or T-SQL command 
        ' -commandParameters - an array of SqlParamters to be cached 
        Public Shared Sub CacheParameterSet(ByVal connectionString As String, _
                                            ByVal commandText As String, _
                                            ByVal ParamArray commandParameters() As SqlParameter)
            Dim hashKey As String = connectionString + ":" + commandText

            paramCache(hashKey) = commandParameters
        End Sub 'CacheParameterSet

        ' retrieve a parameter array from the cache
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection 
        ' -commandText - the stored procedure name or T-SQL command 
        ' Returns: an array of SqlParamters 
        Public Shared Function GetCachedParameterSet(ByVal connectionString As String, ByVal commandText As String) As SqlParameter()
            Dim hashKey As String = connectionString + ":" + commandText
            Dim cachedParameters As SqlParameter() = CType(paramCache(hashKey), SqlParameter())

            If cachedParameters Is Nothing Then
                Return Nothing
            Else
                Return CloneParameters(cachedParameters)
            End If
        End Function 'GetCachedParameterSet

#End Region

#Region "Parameter Discovery Functions"
        ' Retrieves the set of SqlParameters appropriate for the stored procedure
        ' 
        ' This method will query the database for this information, and then store it in a cache for future requests.
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection 
        ' -spName - the name of the stored procedure 
        ' Returns: an array of SqlParameters
        Public Overloads Shared Function GetSpParameterSet(ByVal connectionString As String, ByVal spName As String) As SqlParameter()
            Return GetSpParameterSet(connectionString, spName, False)
        End Function 'GetSpParameterSet 

        ' Retrieves the set of SqlParameters appropriate for the stored procedure
        ' 
        ' This method will query the database for this information, and then store it in a cache for future requests.
        ' Parameters:
        ' -connectionString - a valid connection string for a SqlConnection
        ' -spName - the name of the stored procedure 
        ' -includeReturnValueParameter - a bool value indicating whether the return value parameter should be included in the results 
        ' Returns: an array of SqlParameters 
        Public Overloads Shared Function GetSpParameterSet(ByVal connectionString As String, _
                                                           ByVal spName As String, _
                                                           ByVal includeReturnValueParameter As Boolean) As SqlParameter()

            Dim cachedParameters() As SqlParameter
            Dim hashKey As String

            hashKey = connectionString + ":" + spName + IIf(includeReturnValueParameter = True, ":include ReturnValue Parameter", "")

            cachedParameters = CType(paramCache(hashKey), SqlParameter())

            If (cachedParameters Is Nothing) Then
                paramCache(hashKey) = DiscoverSpParameterSet(connectionString, spName, includeReturnValueParameter)
                cachedParameters = CType(paramCache(hashKey), SqlParameter())

            End If

            Return CloneParameters(cachedParameters)

        End Function 'GetSpParameterSet
#End Region

End Class 'SqlHelperParameterCache 