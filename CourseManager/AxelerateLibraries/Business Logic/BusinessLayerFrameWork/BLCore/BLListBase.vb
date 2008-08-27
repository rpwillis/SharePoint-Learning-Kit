Imports System.Security

Namespace BLCore

    ''' <summary>
    ''' This template defines a BLBusinessBase object collection.  It defines all the strong typed factory methods and collection methods 
    ''' to make the instanced classes able to perform the fetch, update, insert and delete operation on the entire collection.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="C"></typeparam>
    ''' <typeparam name="tDataKey"></typeparam>
    ''' <remarks></remarks>
    <Serializable()> _
    Public MustInherit Class BLListBase( _
        Of T As {BLListBase(Of T, C, tDataKey), New}, _
        C As {BLCore.BLBusinessBase, New}, _
        tDataKey As BLDataKey)
        Inherits BusinessListBase(Of T, C)
        Implements IBLListBase


#Region "Constructors"
        'Creates a new bussines object Collection. it allows to edit, add and remove objects by default
        Protected Sub New()
            AllowEdit = True
            AllowNew = True
            AllowRemove = True
        End Sub
#End Region

#Region "BLListBase Overridables"
        'Must be overriden for returning the type of Criteria (inherits from BLCriteria)
        'defined by the child class.  This Criteria serves as a identifier to find which
        'bussines object are on the Collection
        Protected Overridable ReadOnly Property EmptyCriteria() As BLCriteria
            Get
                Return (New C).KeyCriteria
            End Get
        End Property

        'Adds a new business object empty at the end of the collection
        Public Overridable Sub AddNewBusinessObject()
            Add(New C)
        End Sub

        Public Overloads Function Find(ByVal PropertyName As String, ByVal Value As Object) As BLBusinessBase Implements IBLListBase.Find
            Dim result As BLBusinessBase = Nothing
            Dim itm As BLBusinessBase = Nothing
            For Each itm In Me
                If itm.PropertyValue(PropertyName).Equals(Value) Then
                    result = itm
                    Exit For
                End If
            Next
            Return result
        End Function

        Public Overloads Function Find(ByVal DataKey As BLDataKey) As BLBusinessBase Implements IBLListBase.Find
            Dim result As BLBusinessBase = Nothing
            Dim itm As BLBusinessBase = Nothing
            For Each itm In Me
                If itm.DataKey.Equals(DataKey) Then
                    result = itm
                    Exit For
                End If
            Next
            Return result
        End Function

        Public Overloads Function Find(ByVal DataKey As String) As BLBusinessBase Implements IBLListBase.Find
            Dim result As BLBusinessBase = Nothing
            Dim itm As BLBusinessBase = Nothing
            For Each itm In Me
                If itm.DataKey.ToString() = DataKey Then
                    result = itm
                    Exit For
                End If
            Next
            Return result
        End Function

#End Region

#Region "DataPortal Overrides"
        Protected Overrides Sub DataPortal_Fetch(ByVal Criteria As Object)
            Dim BusinessObject As New C
            Dim SecurityOperation As clsSecurityOperation = BusinessObject.ReadSecurityOperation()
            If SecurityOperation IsNot Nothing Then
                SecurityOperation.DemandAccess()
            End If
            CType(Criteria, BLCriteria).Update()
            BLFetch(CType(Criteria, BLCriteria))
        End Sub

        Protected Overrides Sub DataPortal_Update() Implements IBLListBase.DataPortal_Update
            Dim IndividualSecurityCheck As Boolean = False
            Try
                SecurityCheck("ReadOnly")
            Catch e As System.Exception
                IndividualSecurityCheck = True
            End Try

            m_DataLayerContextInfo = PopSerializationContext()

            Dim Options As New TransactionOptions
            Options.IsolationLevel = Transactions.IsolationLevel.ReadCommitted
            Using scope As TransactionScope = New TransactionScope(TransactionScopeOption.RequiresNew, Options)
                BLUpdate(, IndividualSecurityCheck)
                scope.Complete()
            End Using
        End Sub

        Protected Overrides Sub DataPortal_Delete(ByVal Criteria As Object)
            Dim BusinessObject As New C
            Dim SecurityOperation As clsSecurityOperation = BusinessObject.DeleteSecurityOperation()
            If SecurityOperation IsNot Nothing Then
                SecurityOperation.DemandAccess()
            End If
            CType(Criteria, BLCriteria).Update()
            BusinessObject.BLFetch(CType(Criteria, BLCriteria))

            Dim Options As New TransactionOptions
            Options.IsolationLevel = Transactions.IsolationLevel.ReadCommitted
            Using scope As TransactionScope = New TransactionScope(TransactionScopeOption.RequiresNew, Options)
                BusinessObject.BLDelete()
                scope.Complete()
            End Using
        End Sub
#End Region

#Region "Data Layer Access"
        'Uses the DataLayer loading the bussines object with values
        Public Overridable Sub BLFetch(ByVal Criteria As BLCriteria)

            DataLayerContextInfo = Criteria.DataLayerContextInfo

            Dim IndividualSecurityCheck As Boolean = False
            Try
                SecurityCheck("ReadOnly")
            Catch e As System.Exception
                IndividualSecurityCheck = True
            End Try

            Try
                Dim CachedObjects As List(Of BLFieldMap) = DataLayer.FieldMapList.OnCreationCachedFields(Criteria)
                Dim ReadCommand As DataLayerCommandBase = DataLayer.ReadCommand(Me, Criteria, CachedObjects)
                Try
                    ReadCommand.Execute()
                    'and loop through it to create the child objects
                    While ReadCommand.NextRecord
                        Dim BO As C = New C
                        BO.DataLayerContextInfo = DataLayerContextInfo
                        BO.BLFetchFromCommand(ReadCommand, Criteria)
                        BO.BLFetchClassFields(DataLayer.FieldMapList.ClassFetchFields)
                        If IndividualSecurityCheck Then
                            BO.SecurityCheck("ReadOnly")
                        End If
                        Add(BO)
                    End While

                    FetchCachedObjectsFromCommand(CachedObjects, ReadCommand)
                Catch ex As System.Exception
                    Throw New System.Exception(BLResources.Exception_FetchOperationFailed, ex)
                Finally
                    ReadCommand.Finish()
                End Try

            Catch ex As Exception
                Throw New System.Exception(BLResources.Exception_FetchOperationFailed, ex)
            End Try
        End Sub

        'Reads the information automappable fields from a DataReader that is
        'already positioned in the information of the bussines object
        Friend Sub FetchCachedObjectsFromCommand(ByVal CachedObjects As List(Of BLFieldMap), ByVal Command As DataLayerCommandBase)
            Dim TmpCriteria As New BLCriteria
            For Each FieldMap As BLFieldMap In CachedObjects
                Dim Attr As CachedForeignObjectAttribute = CachedForeignObjectAttribute.GetAttribute(FieldMap.Field)
                Command.NextTable()
                Dim ObjectNumber As Integer = 0
                While Command.NextRecord
                    Dim BO As BLBusinessBase = Attr.NewBusinessObjectInstance(DataLayerContextInfo)
                    BO.DataLayerContextInfo = DataLayerContextInfo
                    Dim ParentBO As BLBusinessBase = Item(ObjectNumber)
                    BO.BLFetchFromCommand(Command, TmpCriteria)
                    ParentBO.CachedPropertyValue(FieldMap.Field) = BO
                    ObjectNumber = ObjectNumber + 1
                End While
            Next
        End Sub

        Public Overridable Sub BLUpdate(Optional ByVal ParentObject As BLBusinessBase = Nothing, _
            Optional ByVal IndividualSecurity As Boolean = False) Implements IBLListBase.BLUpdate
            Dim isFirstUpdateDemand As Boolean = True
            Dim isFirstCreateDemand As Boolean = True
            'Loop through the objects to add and update, calling the Update Method
            For Each child As BLBusinessBase In Me
                If (isFirstCreateDemand AndAlso child.IsNew) Then
                    Dim SecurityOperation As clsSecurityOperation = child.CreateSecurityOperation()
                    If SecurityOperation IsNot Nothing Then
                        SecurityOperation.DemandAccess()
                    End If
                    isFirstCreateDemand = False
                End If
                If (isFirstUpdateDemand AndAlso (child.IsNew = False)) Then
                    Dim SecurityOperation As clsSecurityOperation = child.UpdateSecurityOperation()
                    If SecurityOperation IsNot Nothing Then
                        SecurityOperation.DemandAccess()
                    End If
                    isFirstUpdateDemand = False
                End If
                child.DataLayerContextInfo = DataLayerContextInfo
                child.BLSmartUpdate(ParentObject)
            Next
        End Sub

        ''' <summary>
        ''' Saves the object to the database.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Calling this method starts the save operation, causing the all child
        ''' objects to be inserted, updated or deleted within the database based on the
        ''' each object's current state.
        ''' </para><para>
        ''' All this is contingent on <see cref="IsDirty" />. If
        ''' this value is <see langword="false"/>, no data operation occurs. 
        ''' It is also contingent on <see cref="IsValid" />. If this value is 
        ''' <see langword="false"/> an exception will be thrown to 
        ''' indicate that the UI attempted to save an invalid object.
        ''' </para><para>
        ''' It is important to note that this method returns a new version of the
        ''' business collection that contains any data updated during the save operation.
        ''' You MUST update all object references to use this new version of the
        ''' business collection in order to have access to the correct object data.
        ''' </para><para>
        ''' You can override this method to add your own custom behaviors to the save
        ''' operation. For instance, you may add some security checks to make sure
        ''' the user can save the object. If all security checks pass, you would then
        ''' invoke the base Save method via <c>MyBase.Save()</c>.
        ''' </para>
        ''' </remarks>
        ''' <returns>A new object containing the saved values.</returns>
        Public Overrides Function Save() As T

            If Me.IsChild Then
                Throw New Exception(BLResources.Exception_InvalidChildSave)
            End If

            If Not IsValid Then
                Throw New Exception(BLResources.Exception_InvalidStateSave)
            End If

            If IsDirty Or MarkDirtyRecursive() Then
                If Not DataLayerContextInfo.IsDefaultDataLayer Then
                    PushSerializationContext(DataLayerContextInfo)
                End If

                Dim TmpObject As IBLListBase = CType(GetClone(), T)
                TmpObject.DataPortal_Update()
                PopSerializationContext()
                Return CType(TmpObject, T)
            Else
                Return DirectCast(Me, T)
            End If

        End Function


#End Region

#Region "Data Layer Definition"
        ''' <summary>
        ''' Information of the DataLayer creation
        ''' </summary>
        ''' <remarks></remarks>
        <NonSerialized()> _
        Private m_DataLayerContextInfo As DataLayerContextInfo

        ''' <summary>
        ''' Specific instance of the DataLayer going to be used
        ''' </summary>
        ''' <remarks></remarks>
        <NonSerialized()> _
        Private m_DataLayer As DataLayerAbstraction = Nothing

        ''' <summary>
        ''' Manages a cache for the creation of the DataLayer going to be used in the operations of this Collection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Property DataLayerContextInfo() As DataLayerContextInfo Implements IBLListBase.DataLayerContextInfo
            Get
                If m_DataLayerContextInfo Is Nothing Then
                    m_DataLayerContextInfo = New DataLayerContextInfo(GetType(C))
                End If
                Return m_DataLayerContextInfo
            End Get
            Set(ByVal value As DataLayerContextInfo)
                m_DataLayerContextInfo = value
            End Set
        End Property

        ''' <summary>
        ''' Manages a cache for the creation of the DataLayer going to be used in the operations of this Collection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property DataLayer() As DLCore.DataLayerAbstraction Implements IBLListBase.DataLayer
            Get
                If m_DataLayer Is Nothing Then
                    Dim BO As New C
                    BO.DataLayerContextInfo = DataLayerContextInfo
                    m_DataLayer = BO.DataLayer
                End If
                Return m_DataLayer

            End Get
            Set(ByVal value As DLCore.DataLayerAbstraction)
                m_DataLayer = value
                DataLayerContextInfo.DataLayerType = value.GetType
            End Set
        End Property

#End Region


#Region "Security Properties and Methods"

        Protected Friend Sub SecurityCheck(ByVal NivelAcceso As String) Implements IBLListBase.SecurityCheck
            'Dim MyPrincipal As MyGenericPrincipal = CType(System.Threading.Thread.CurrentPrincipal, MyGenericPrincipal)
            'Dim BusinessObjectName As String = Me.NewBusinessObject.GetType.Name
            'Try

            '    Dim DemandString As String = BusinessObjectName + "//" + NivelAcceso
            '    Dim MyPermission As Permissions.PrincipalPermission
            '    MyPermission = New Permissions.PrincipalPermission(Nothing, DemandString)
            '    MyPermission.Demand()
            'Catch e As System.Exception
            '    Throw New System.Exception("El usuario no tiene el nivel de acceso adecuado (" + NivelAcceso + ") para realizar la operación sobre " + BusinessObjectName)
            'Finally
            'End Try

        End Sub

#End Region

#Region "Métodos para Web Data Binding"
        Public Sub DataSource_Insert(ByVal BusinessObject As C)
            BusinessObject.Save()
        End Sub

        Public Sub DataSource_Update(ByVal BusinessObject As C)
            BusinessObject.Save(True)
        End Sub

        Public Sub DataSource_Delete(ByVal BusinessObject As C)
            BusinessObject.Delete()
            BusinessObject.Save(True)
        End Sub
#End Region

#Region "Uncategorized"
        'Allows to edit elements in the collection
        Public Sub AllowEdition(Optional ByVal Allow As Boolean = True)
            AllowEdit = Allow
        End Sub

        'Allows to remove elements in the collection
        Public Sub AllowRemoving(Optional ByVal Allow As Boolean = True)
            AllowRemove = Allow
        End Sub

        Public Overridable Function NewBusinessObject() As BLBusinessBase Implements IBLListBase.NewBusinessObject
            Dim BO As C = New C
            Return BO
        End Function

        Protected Function MarkDirtyRecursive() As Boolean Implements IBLListBase.MarkDirtyRecursive
            Dim NeedsMarkingAsDirty As Boolean = False

            If Me.Count > 0 Then
                For Each BusinessObject As BLBusinessBase In Me
                    If BusinessObject.MarkDirtyRecursive Then
                        NeedsMarkingAsDirty = True
                    End If
                Next
            End If
            Return NeedsMarkingAsDirty
        End Function

#End Region

#Region "Factory Methods"
        Public Sub Local_GetCollection(ByVal Criteria As BLCriteria) Implements IBLListBase.Local_GetCollection
            DataLayerContextInfo.ForceLocal = True
            DataPortal_Fetch(Criteria)
        End Sub

        Public Class CollectionFactory(Of tDataLayerType As DataLayerAbstraction)

            ''' <summary>
            ''' Returns a new bussines object Collection empty
            ''' </summary>
            ''' <param name="NDataLayerContextInfo"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <staFactory()> _
            Public Shared Function NewCollection(Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As T
                Dim BC As IBLListBase = CType(New T, IBLListBase)
                If Not NDataLayerContextInfo Is Nothing Then
                    BC.DataLayerContextInfo.Copy(NDataLayerContextInfo, GetType(C))
                    If BC.DataLayerContextInfo.DataLayerType Is Nothing Then
                        BC.DataLayerContextInfo.DataLayerType = GetType(tDataLayerType)
                    End If
                End If
                Return CType(BC, T)
            End Function

            ''' <summary>
            ''' Returns a collection based on a Criteria. This Function is called by the other GetCollection
            ''' </summary>
            <staFactory()> _
            Public Shared Function GetCollection(ByVal Criteria As BusinessListCriteria) As T
                If Criteria.DataLayerContextInfo.DataLayerType Is Nothing Then
                    Criteria.DataLayerContextInfo.DataLayerType = GetType(tDataLayerType)
                End If
                Dim BC As IBLListBase = CType(New T, IBLListBase)
                BC.DataLayerContextInfo.Copy(Criteria.DataLayerContextInfo, GetType(C))
                BC.Local_GetCollection(Criteria)
                Return CType(BC, T)
            End Function

            <staFactory()> _
            Public Shared Function GetCollection(ByVal DataKey As tDataKey, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As T
                'Dim Criteria As New BLListBase(Of tBusinessObject, tDataKey).Criteria()
                Dim Criteria As New BusinessListCriteria
                Criteria.DataLayerContextInfo = NDataLayerContextInfo
                DataKey.AddToCriteria(Criteria)
                Return GetCollection(Criteria)
            End Function

            <staFactory()> _
            Public Shared Function GetCollection(ByVal NCriteria As BLCriteria) As T
                NCriteria.Update()
                Dim Criteria As New BusinessListCriteria(NCriteria)
                Return GetCollection(Criteria)
            End Function

            <staFactory()> _
            Public Shared Function GetCollection(Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As T
                Dim Criteria As New BusinessListCriteria()
                Criteria.DataLayerContextInfo = NDataLayerContextInfo
                Return GetCollection(Criteria)
            End Function

            ''' <summary>
            ''' Returns a collection based on a Criteria. This Function receives a paging size and a sorting attribute
            ''' </summary>
            ''' <param name="Criteria"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <staFactory("SortFieldName", "")> _
            Public Shared Function GetCollection(ByVal Criteria As BusinessListCriteria, ByVal PageSize As Integer, ByVal PageNumber As Integer, ByVal SortFieldName As String, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As T
                Criteria.PageSize = PageSize
                Criteria.PageNumber = PageNumber
                Criteria.AddOrderedField(SortFieldName, True)
                Criteria.DataLayerContextInfo = NDataLayerContextInfo
                Return GetCollection(Criteria)
            End Function
        End Class

        ''' <summary>
        ''' Returns a new bussines collection empty
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <staFactory()> _
        Public Shared Function NewCollection() As T
            Dim BC As IBLListBase = CType(New T, IBLListBase)
            Return CType(BC, T)
        End Function

        ''' <summary>
        ''' Returns a new bussines collection empty
        ''' </summary>
        ''' <param name="NDataLayerContextInfo"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <staFactory()> _
        Public Shared Function NewCollection(ByVal NDataLayerContextInfo As DataLayerContextInfo) As T
            Dim BC As IBLListBase = CType(New T, IBLListBase)
            If Not NDataLayerContextInfo Is Nothing Then
                BC.DataLayerContextInfo.Copy(NDataLayerContextInfo, GetType(C))
            End If
            Return CType(BC, T)
        End Function

        ''' <summary>
        ''' Returns a collection based on a Criteria. This Function is called by the other GetCollection
        ''' </summary>
        ''' <param name="Criteria"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <staFactory()> _
        Public Shared Function GetCollection(ByVal Criteria As BusinessListCriteria) As T
            Dim BC As IBLListBase = CType(New T, IBLListBase)
            BC.DataLayerContextInfo.Copy(Criteria.DataLayerContextInfo, GetType(C))
            BC.Local_GetCollection(Criteria)
            Return CType(BC, T)
        End Function
        <staFactory()> _
        Public Shared Function GetCollection(ByVal DataKey As tDataKey, ByVal NDataLayerCreationInfo As DataLayerContextInfo) As T
            'Dim Criteria As New BLListBase(Of tBusinessObject, tDataKey).Criteria()
            Dim Criteria As New BusinessListCriteria
            DataKey.AddToCriteria(Criteria)
            Criteria.DataLayerContextInfo = NDataLayerCreationInfo
            Return GetCollection(Criteria)
        End Function
        <staFactory()> _
        Public Shared Function GetCollection(ByVal DataKey As tDataKey) As T
            'Dim Criteria As New BLListBase(Of tBusinessObject, tDataKey).Criteria()
            Dim Criteria As New BusinessListCriteria
            DataKey.AddToCriteria(Criteria)
            Return GetCollection(Criteria)
        End Function

        <staFactory()> _
        Public Shared Function GetCollection(ByVal NCriteria As BLCriteria) As T
            NCriteria.Update()
            Dim Criteria As New BusinessListCriteria(NCriteria)
            Return GetCollection(Criteria)
        End Function

        <staFactory()> _
        Public Shared Function GetCollection(ByVal NDataLayerCreationInfo As DataLayerContextInfo) As T
            Dim Criteria As New BusinessListCriteria()
            Criteria.DataLayerContextInfo = NDataLayerCreationInfo
            Return GetCollection(Criteria)
        End Function

        <staFactory()> _
        Public Shared Function GetCollection() As T
            Dim Criteria As New BusinessListCriteria()
            Return GetCollection(Criteria)
        End Function

        ''' <summary>
        ''' Returns a collection based on a Criteria. This Function receives a paging size and a sorting attribute
        ''' </summary>
        <staFactory("SortFieldName", "")> _
        Public Shared Function GetCollection(ByVal Criteria As BLCriteria, ByVal SortFieldName As String, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As T
            If (SortFieldName <> "") Then
                Criteria.AddOrderedField(SortFieldName, True)
            End If
            Criteria.DataLayerContextInfo = NDataLayerContextInfo
            Return GetCollection(Criteria)
        End Function


        ''' <summary>
        ''' Returns a collection based on a Criteria. This Function receives a paging size and a sorting attribute
        ''' </summary>
        <staFactory("SortFieldName", "")> _
        Public Shared Function GetCollection(ByVal Criteria As BLCriteria, ByVal PageSize As Integer, ByVal StartRowIndex As Integer, ByVal SortFieldName As String, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As T
            If PageSize <> 0 Then
                Criteria.PageSize = PageSize
                Criteria.PageNumber = CInt(StartRowIndex / PageSize)
            End If

            If (SortFieldName <> "") Then
                Criteria.AddOrderedField(SortFieldName, True)
            End If
            Criteria.DataLayerContextInfo = NDataLayerContextInfo
            Return GetCollection(Criteria)
        End Function


        Public Shared Function GetCount(ByVal Criteria As BLCriteria, ByVal PageSize As Integer, ByVal StartRowIndex As Integer, ByVal SortFieldName As String, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As Integer
            Criteria.DataLayerContextInfo = NDataLayerContextInfo
            Dim CountCommand As BLCountCommand = New BLCountCommand(GetType(T), Criteria)
            CountCommand.Execute()
            Return CountCommand.Count
        End Function


        Public Shared Function GetCount(ByVal Criteria As BLCriteria, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As Integer
            Criteria.DataLayerContextInfo = NDataLayerContextInfo
            Dim CountCommand As BLCountCommand = New BLCountCommand(GetType(T), Criteria)
            CountCommand.Execute()
            Return CountCommand.Count
        End Function


        Public Shared Function GetCount(Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As Integer
            Dim Criteria As BLCriteria = New BLCriteria(GetType(T))
            Criteria.DataLayerContextInfo = NDataLayerContextInfo
            Dim CountCommand As BLCountCommand = New BLCountCommand(GetType(T), Criteria)
            CountCommand.Execute()
            Return CountCommand.Count
        End Function

        <staFactory()> _
        Friend Shared Function GetCollection(ByVal ReadCommand As DataLayerCommandBase, Optional ByVal Local As Boolean = False) As T
            Dim col As New T
            'TODO:  Needs to be implemented
            Return col
        End Function

        Public Shared Function GetValueList(ByVal FieldNames() As String, ByVal Criteria As BLCriteria) As DataSet
            Dim ValueListCommand As New BLValueListCommand(GetType(T), FieldNames, Criteria)
            ValueListCommand.Execute()
            Return ValueListCommand.Result
        End Function
#End Region

#Region "List Methods"
        Public Overloads Function Contains(ByVal DataKey As tDataKey) As Boolean
            For Each BO As C In Me
                If BO.DataKey.Equals(DataKey) Then
                    Return True
                End If
            Next
            Return False
        End Function
#End Region

#Region " Criteria Class (identifies the Individual Object/ Primary Key) "

        'Redefines the class AutoMapCriteria  so it could identified a new bussines collection
        <Serializable()> _
        Public Class BusinessListCriteria
            Inherits BLCriteria

            'Builds a new object empty
            Public Sub New()
                MyBase.New(GetType(T))
            End Sub

            'Builds as NAutoMapCriteria 
            Public Sub New(ByVal Criteria As BLCriteria)
                MyBase.New(Criteria, GetType(T))
            End Sub


        End Class

#End Region ' Criteria

#Region "DataLayer Serialization Context"
        Private m_DataLayerSerializationContext As DLCore.DataLayerContextInfo = Nothing

        Public Sub PushSerializationContext(ByVal SerializationContext As DLCore.DataLayerContextInfo)
            m_DataLayerSerializationContext = SerializationContext
        End Sub

        Public Function PopSerializationContext() As DLCore.DataLayerContextInfo
            Dim ToReturn As DLCore.DataLayerContextInfo = m_DataLayerSerializationContext
            m_DataLayerSerializationContext = Nothing

            'Assures that the DataLayer is properly loaded
            Dim DataLayerInstance As DataLayerAbstraction = DataLayer

            Return ToReturn
        End Function

#End Region

    End Class
End Namespace