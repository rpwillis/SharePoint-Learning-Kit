Imports Microsoft.Interop.Security.AzRoles
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Security.Principal

Namespace BLCore.Security
    Public Class clsAzmanUtility

        Private Shared m_EnableSecurity As Boolean

        Private Shared m_Store As IAzAuthorizationStore

        Private Shared m_Application As IAzApplication

        Private Shared m_Operations As Hashtable = New Hashtable()
        ''' <summary>
        ''' Defines if the security is enable.
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared Property EnableSecurity() As Boolean
            Get
                Return m_EnableSecurity
            End Get
            Set(ByVal value As Boolean)
                m_EnableSecurity = value
            End Set
        End Property
        ''' <summary>
        ''' The Azman store
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared Property Store() As IAzAuthorizationStore
            Get
                Return m_Store
            End Get
            Set(ByVal value As IAzAuthorizationStore)
                m_Store = value
            End Set
        End Property
        ''' <summary>
        ''' The Azman Application
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared Property Application() As IAzApplication
            Get
                Return m_Application
            End Get
            Set(ByVal value As IAzApplication)
                m_Application = value
            End Set
        End Property
        ''' <summary>
        ''' The Operations of the Azman Application
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared Property Operations() As Hashtable
            Get
                Return m_Operations
            End Get
            Set(ByVal value As Hashtable)
                m_Operations = value
            End Set
        End Property
        ''' <summary>
        ''' Deletes the operations in the Azman Application
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared Sub DeleteOperations()
            Operations.Clear()
            For Each Operation As IAzOperation In Operations.Values
                Application.DeleteOperation(Operation.Name)
            Next
        End Sub
        ''' <summary>
        ''' Saves the operations of the selected assembly in the Azman Application
        ''' </summary>
        ''' <param name="NewAssembly">The name of the Assembly</param>
        ''' <remarks></remarks>
        Public Shared Sub SaveOperations(ByVal NewAssembly As Assembly)

            For Each AssemblyClass As Type In NewAssembly.GetTypes()

                If (ReflectionHelper.InheritsFrom(GetType(BLBusinessBase), AssemblyClass)) Then
                    Try
                        Dim BusinessBase As BLBusinessBase = CType(Activator.CreateInstance(AssemblyClass), BLBusinessBase)
                        Dim SecurityOperations As List(Of clsSecurityOperation) = BusinessBase.GetSecurityOperations()

                        For Each Operation As clsSecurityOperation In SecurityOperations
                            If (ReflectionHelper.InheritsFrom(GetType(clsAzmanOperation), Operation.GetType())) Then
                                SaveOperation(Operation.Name)
                            End If
                        Next
                    Catch ex As Exception
                        'continue
                    End Try

                End If
            Next
        End Sub

        ''' <summary>
        ''' Saves a Operation in the Azman Application
        ''' </summary>
        ''' <param name="Name">Name of the Operation to save</param>
        ''' <remarks></remarks>
        Private Shared Sub SaveOperation(ByVal Name As String)
            Try

                If Operations.Contains(Name) = False Then
                    Dim OperationID As Integer = Application.Operations.Count + 1
                    Dim Operation As IAzOperation = Application.CreateOperation(Name)

                    Operation.OperationID = OperationID
                    Operation.Submit()

                    Operations.Add(Operation.Name, Operation)
                End If
            Catch ex As Exception
                Throw ex
            End Try

        End Sub

        ''' <summary>
        ''' Loads all operations of the Azman application, in a hashtable
        ''' </summary>
        ''' <remarks></remarks>
        Private Shared Sub LoadOperationsFromAzman()
            Operations.Clear()
            Dim OperationIndex As Integer = 1
            For Each Operation As IAzOperation In Application.Operations
                Operations.Add(Operation.Name, Operation)
            Next
        End Sub

        ''' <summary>
        ''' Initializes the AzmanUtility
        ''' </summary>
        ''' <remarks></remarks>
        Shared Sub New()
            Dim AzmanApplication As String
            Dim AzmanStore As String
            'Information about the azman application and store in the config file
            AzmanApplication = System.Configuration.ConfigurationManager.AppSettings("AzmanApplication")
            AzmanStore = System.Configuration.ConfigurationManager.AppSettings("AzmanStore")
            Dim EnableSecurityString As String = System.Configuration.ConfigurationManager.AppSettings("EnableSecurity")
            EnableSecurity = True
            If ((EnableSecurityString IsNot Nothing) AndAlso (EnableSecurityString.ToLower.CompareTo("false") = 0)) Then
                EnableSecurity = False
            Else
                'Opens the azman store and the application
                OpenApplication(AzmanStore, AzmanApplication)
            End If
        End Sub

        ''' <summary>
        ''' Initializes the Azman Store
        ''' </summary>
        ''' <param name="AzmanStore">The name of the Azman Store</param>
        ''' <remarks></remarks>
        Private Shared Sub InitializeStore(ByVal AzmanStore As String)
            Store = New AzAuthorizationStoreClass()
            Try
                Store.Initialize(0, AzmanStore)
            Catch ex As Exception
                Throw New Exception("The Store(" + AzmanStore + ") cannot be initialized", ex)
            End Try
        End Sub

        ''' <summary>
        ''' Opens the selected Application located in the Azman Store.
        ''' </summary>
        ''' <param name="AzmanStore">Name of the Azman Store</param>
        ''' <param name="AzmanApplication">Name of the Azman Application</param>
        ''' <remarks></remarks>
        Private Shared Sub OpenApplication(ByVal AzmanStore As String, ByVal AzmanApplication As String)
            InitializeStore(AzmanStore)
            Try
                Application = Store.OpenApplication(AzmanApplication)
                LoadOperationsFromAzman()
            Catch ex As Exception
                ReleaseObject(Store)
                Store = Nothing
                Throw New Exception(My.Resources.BLResources.theApplication + AzmanApplication + My.Resources.BLResources.cannotBeInitialized, ex)

            End Try
        End Sub

        ''' <summary>
        ''' Loads the context of the selected user.
        ''' </summary>
        ''' <param name="Name">The name of the user</param>
        ''' <returns>Context of the user</returns>
        ''' <remarks></remarks>
        Private Shared Function LoadContext(ByVal Name As String) As IAzClientContext
            Dim Context As IAzClientContext

            If Name Is "" Then
                Throw New Exception(My.Resources.BLResources.errContextCannotInitialize)
            Else
                Dim RegularExpression As Regex = New Regex("^([\w]+)\\([\w]+)$")
                Dim MatchExpression As Match = RegularExpression.Match(Name)
                If MatchExpression.Success Then
                    Dim Domain As String = MatchExpression.Groups(1).Value
                    Dim User As String = MatchExpression.Groups(2).Value
                    Try
                        Context = Application.InitializeClientContextFromName(User, Domain)
                    Catch ex As Exception
                        Throw New Exception(My.Resources.BLResources.theContext + Name + My.Resources.BLResources.cannotBeInitialized, ex)
                    End Try
                Else
                    Throw New Exception(My.Resources.BLResources.theContext + Name + My.Resources.BLResources.cannotBeInitialized)
                End If
            End If
            Return Context
        End Function
        ''' <summary>
        ''' Loads the context of current user
        ''' </summary>
        ''' <returns>The current context</returns>
        ''' <remarks></remarks>
        Private Shared Function LoadContext() As IAzClientContext
            Dim Context As IAzClientContext
            Dim Token As ULong
            If HttpContext.Current Is Nothing Then

                Token = 0
            Else
                Dim TokenPtr As IntPtr = CType(HttpContext.Current.User.Identity, WindowsIdentity).Token
                Token = CType(Token, ULong)
            End If
            Try
                Context = Application.InitializeClientContextFromToken(Token)
            Catch ex As Exception
                Throw New Exception(My.Resources.BLResources.ContextCannotInitializeByToken)
            End Try

            Return Context

        End Function

        ''' <summary>
        ''' Release the selected com object
        ''' </summary>
        ''' <param name="obj">The name of the com object</param>
        ''' <remarks></remarks>
        Private Shared Sub ReleaseObject(ByVal obj As Object)
            If Not (obj Is Nothing) Then
                While Not (System.Runtime.InteropServices.Marshal.ReleaseComObject(obj) = 0)
                End While
            End If

        End Sub
        ''' <summary>
        ''' Checks if the current user has enough rights to perform the role
        ''' </summary>
        ''' <param name="RoleName">The name of the role</param>
        ''' <returns>Returns true if the user has enough rights to perform the role</returns>
        ''' <remarks></remarks>
        Public Shared Function CheckRoleAccess(ByVal RoleName As String) As Boolean

            Dim Context As IAzClientContext = LoadContext()

            For Each Role As String In CType(Context.GetRoles(), Object())
                If Role = RoleName Then
                    Return True
                End If
            Next
            Return False
        End Function
        ''' <summary>
        ''' Checks if the current user has enough rights to perform the array of operations
        ''' </summary>
        ''' <param name="OperationValues">Array of objects with the id of the operations to check</param>
        ''' <returns>A array of objects with the check of each operation</returns>
        ''' <remarks></remarks>
        Public Shared Function CheckAccess(ByVal OperationValues() As Object) As Object()
            If EnableSecurity = False Then
                Dim Result() As Object = {0}
                Return Result
            End If
            Dim Context As IAzClientContext = LoadContext()
            Dim obj As Object = Context.GetRoles()
            Dim Scopes() As Object = {""}
            Dim Results() As Object
            Try
                Results = CType(Context.AccessCheck(My.Resources.BLResources.checkAccessOp, Scopes, OperationValues), Object())

            Catch ex As Exception
                ReleaseObject(Context)
                Context = Nothing
                Throw New Exception(My.Resources.BLResources.checkAccessFailed)
            End Try
            ReleaseObject(Context)
            Context = Nothing
            Return Results

        End Function

        ''' <summary>
        ''' Checks if the current user has enough rights to perform the operation
        ''' </summary>
        ''' <param name="OperationID">The ID of the operation</param>
        ''' <returns>Returns true if the user has enough rights to perform the operation</returns>
        ''' <remarks></remarks>
        Public Shared Function CheckAccess(ByVal OperationID As Integer) As Boolean
            Dim AuxOperations(0) As Object
            AuxOperations.SetValue(OperationID, 0)
            Dim Result As Integer = CType(CheckAccess(AuxOperations)(0), Integer)
            If Result = 0 Then
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' Checks if the current user has enough rights to perform the operation
        ''' </summary>
        ''' <param name="OperationName">The name of the operation</param>
        ''' <returns>Returns true if the user has enough rights to perform the operation</returns>
        ''' <remarks></remarks>
        Public Shared Function CheckAccess(ByVal OperationName As String) As Boolean
            Dim OperationID As Integer = GetOperationID(OperationName)
            Dim Result As Boolean = CheckAccess(OperationID)
            Return Result
        End Function

        ''' <summary>
        ''' Gets the ID of the operation
        ''' </summary>
        ''' <param name="OperationName">The name of the operation</param>
        ''' <returns>The ID of the Operation</returns>
        ''' <remarks></remarks>
        Public Shared Function GetOperationID(ByVal OperationName As String) As Integer
            If EnableSecurity = False Then
                Return 0
            End If
            Dim Operation As IAzOperation = CType(Operations.Item(OperationName), IAzOperation)
            Return Operation.OperationID
        End Function
        ''' <summary>
        ''' Revokes access to the current user
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub RevokeAccess()


        End Sub
        ''' <summary>
        ''' Grants access to the current user
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Sub GrantAccess()


        End Sub

        ''' <summary>
        ''' Deletes the Azman Store
        ''' </summary>
        ''' <param name="StoreName">The name of the store to be deleted</param>
        ''' <remarks></remarks>
        Private Shared Sub DeleteStore(ByVal StoreName As String)
            Store = New AzAuthorizationStoreClass()

            Try
                Store.Initialize(0, StoreName)
            Catch ex As Exception
                Throw New Exception(My.Resources.BLResources.theStore + StoreName + My.Resources.BLResources.cannotBeInitialized, ex)
            End Try
            Try
                Store.Delete()
                Store.Submit()
                ReleaseObject(Store)
                ReleaseObject(Application)
                Operations.Clear()
            Catch ex As Exception
                Throw New Exception(My.Resources.BLResources.theStore + StoreName + My.Resources.BLResources.cannotBeDeleted, ex)
            End Try
        End Sub
    End Class

End Namespace