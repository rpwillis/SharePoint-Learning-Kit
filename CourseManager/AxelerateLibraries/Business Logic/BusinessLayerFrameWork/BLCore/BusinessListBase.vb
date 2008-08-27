Imports System.ComponentModel

Namespace BLCore

    ''' <summary>
    ''' This is the base class from which most business collections
    ''' or lists will be derived.
    ''' </summary>
    ''' <typeparam name="T">Type of the business object being defined.</typeparam>
    ''' <typeparam name="C">Type of the child objects contained in the list.</typeparam>
    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")> _
    <Serializable()> _
    Public MustInherit Class BusinessListBase( _
      Of T As BusinessListBase(Of T, C), C As BLBusinessBase)
        Inherits System.ComponentModel.BindingList(Of C)
        Implements ICloneable

#Region " Constructors "

        Protected Sub New()

            Initialize()

        End Sub

#End Region

#Region " Initialize "

        ''' <summary>
        ''' Override this method to set up event handlers so user
        ''' code in a partial class can respond to events raised by
        ''' generated code.
        ''' </summary>
        Protected Overridable Sub Initialize()
            ' allows a generated class to set up events to be
            ' handled by a partial class containing user code
        End Sub

#End Region

#Region " IsDirty, IsValid "

        ''' <summary>
        ''' Gets a value indicating whether this object's data has been changed.
        ''' </summary>
        Public ReadOnly Property IsDirty() As Boolean
            Get
                ' run through all the child objects
                ' and if any are dirty then the
                ' collection is dirty
                For Each Child As C In Me
                    If Child.IsDirty Then Return True
                Next
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this object is currently in
        ''' a valid state (has no broken validation rules).
        ''' </summary>
        Public Overridable ReadOnly Property IsValid() As Boolean
            Get
                ' run through all the child objects
                ' and if any are invalid then the
                ' collection is invalid
                For Each child As C In Me
                    If Not child.IsValid Then Return False
                Next
                Return True
            End Get
        End Property

#End Region

#Region " Insert, Remove, Clear "

        ''' <summary>
        ''' Marks the child object for deletion and moves it to
        ''' the collection of deleted objects.
        ''' </summary>
        Protected Overrides Sub RemoveItem(ByVal index As Integer)
            ' when an object is 'removed' it is really
            ' being deleted, so do the deletion work
            MyBase.RemoveItem(index)
        End Sub

        ''' <summary>
        ''' Clears the collection, moving all active
        ''' items to the deleted list.
        ''' </summary>
        Protected Overrides Sub ClearItems()
            While MyBase.Count > 0
                RemoveItem(0)
            End While
            MyBase.ClearItems()
        End Sub

        ''' <summary>
        ''' Replaces the item at the specified index with
        ''' the specified item, first moving the original
        ''' item to the deleted list.
        ''' </summary>
        ''' <param name="index">The zero-based index of the item to replace.</param>
        ''' <param name="item">
        ''' The new value for the item at the specified index. 
        ''' The value can be null for reference types.
        ''' </param>
        ''' <remarks></remarks>
        Protected Overrides Sub SetItem(ByVal index As Integer, ByVal item As C)
            ' copy the original object to the deleted list,
            ' marking as deleted, etc.
            ' replace the original object with this new
            ' object
            MyBase.SetItem(index, item)
        End Sub

#End Region

#Region " IsChild "

        Private mIsChild As Boolean = False

        ''' <summary>
        ''' Indicates whether this collection object is a child object.
        ''' </summary>
        ''' <returns>True if this is a child object.</returns>
        Protected ReadOnly Property IsChild() As Boolean
            Get
                Return mIsChild
            End Get
        End Property

        ''' <summary>
        ''' Marks the object as being a child object.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' By default all business objects are 'parent' objects. This means
        ''' that they can be directly retrieved and updated into the database.
        ''' </para><para>
        ''' We often also need child objects. These are objects which are contained
        ''' within other objects. For instance, a parent Invoice object will contain
        ''' child LineItem objects.
        ''' </para><para>
        ''' To create a child object, the MarkAsChild method must be called as the
        ''' object is created. Please see Chapter 7 for details on the use of the
        ''' MarkAsChild method.
        ''' </para>
        ''' </remarks>
        Protected Sub MarkAsChild()
            mIsChild = True
        End Sub

#End Region

#Region " ICloneable "

        Private Function ICloneable_Clone() As Object Implements ICloneable.Clone

            Return GetClone()

        End Function

        ''' <summary>
        ''' Creates a clone of the object.
        ''' </summary>
        ''' <returns>
        ''' A new object containing the exact data of the original object.
        ''' </returns>
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected Overridable Function GetClone() As Object

            Return ObjectCloner.Clone(Me)

        End Function

        ''' <summary>
        ''' Creates a clone of the object.
        ''' </summary>
        ''' <returns>
        ''' A new object containing the exact data of the original object.
        ''' </returns>
        Public Overloads Function Clone() As T

            Return DirectCast(GetClone(), T)

        End Function

#End Region

#Region " Cascade Child events "

        Private Sub Child_PropertyChanged(ByVal sender As Object, _
          ByVal e As System.ComponentModel.PropertyChangedEventArgs)

            For index As Integer = 0 To Count - 1
                If ReferenceEquals(Me(index), sender) Then
                    OnListChanged(New System.ComponentModel.ListChangedEventArgs( _
                      ComponentModel.ListChangedType.ItemChanged, index))
                    Exit For
                End If
            Next

        End Sub

#End Region

#Region " Serialization Notification "

        <OnDeserialized()> _
        Private Sub OnDeserializedHandler(ByVal context As StreamingContext)

            For Each child As C In Me
                Dim c As System.ComponentModel.INotifyPropertyChanged = TryCast(child, System.ComponentModel.INotifyPropertyChanged)
                If c IsNot Nothing Then
                    AddHandler c.PropertyChanged, AddressOf Child_PropertyChanged
                End If
            Next
            OnDeserialized(context)

        End Sub

        ''' <summary>
        ''' This method is called on a newly deserialized object
        ''' after deserialization is complete.
        ''' </summary>
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected Overridable Sub OnDeserialized(ByVal context As StreamingContext)

            ' do nothing - this is here so a subclass
            ' could override if needed

        End Sub

#End Region

#Region " Data Access "

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
        Public Overridable Function Save() As T

            If Me.IsChild Then
                Throw New NotSupportedException(BLResources.NoSaveChildException)
            End If

            'If mEditLevel > 0 Then
            '    Throw New ValidationException(BLResources.NoSaveEditingException)
            'End If

            If Not IsValid Then
                Throw New Exception(BLResources.NoSaveInvalidException)
                'Throw New ValidationException(BLResources.NoSaveInvalidException)
            End If

            If IsDirty Then
                Return DirectCast(DataPortal_UpdateFunction(), T)
                'Return DirectCast(DataPortal.Update(Me), T)

            Else
                Return DirectCast(Me, T)
            End If

        End Function

        ''' <summary>
        ''' Override this method to load a new business object with default
        ''' values from the database.
        ''' </summary>
        ''' <param name="Criteria">An object containing criteria values.</param>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        Protected Overridable Sub DataPortal_Create(ByVal criteria As Object)
            Throw New NotSupportedException(BLResources.NotSupportedException_Create)
        End Sub

        ''' <summary>
        ''' Override this method to allow retrieval of an existing business
        ''' object based on data in the database.
        ''' </summary>
        ''' <param name="Criteria">An object containing criteria values to identify the object.</param>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        Protected Overridable Sub DataPortal_Fetch(ByVal criteria As Object)
            Throw New NotSupportedException(BLResources.NotSupportedException_Fetch)
        End Sub

        ''' <summary>
        ''' Override this method to allow insert, update or deletion of a business
        ''' object.
        ''' </summary>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        Protected Overridable Sub DataPortal_Update()
            Throw New NotSupportedException(BLResources.NotSupportedException_Update)
        End Sub
        Public Overridable Function DataPortal_UpdateFunction() As T
            Dim TmpObject As IBLListBase = CType(GetClone(), IBLListBase)
            TmpObject.DataPortal_Update()
            Return CType(TmpObject, T)
        End Function

        ''' <summary>
        ''' Override this method to allow immediate deletion of a business object.
        ''' </summary>
        ''' <param name="Criteria">An object containing criteria values to identify the object.</param>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        Protected Overridable Sub DataPortal_Delete(ByVal criteria As Object)
            Throw New NotSupportedException(BLResources.NotSupportedException_Delete)
        End Sub

        ''' <summary>
        ''' Called by the server-side DataPortal prior to calling the 
        ''' requested DataPortal_xyz method.
        ''' </summary>
        ''' <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected Overridable Sub DataPortal_OnDataPortalInvoke(ByVal e As EventArgs)

        End Sub

        ''' <summary>
        ''' Called by the server-side DataPortal after calling the 
        ''' requested DataPortal_xyz method.
        ''' </summary>
        ''' <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected Overridable Sub DataPortal_OnDataPortalInvokeComplete(ByVal e As EventArgs)

        End Sub

        ''' <summary>
        ''' Called by the server-side DataPortal if an exception
        ''' occurs during data access.
        ''' </summary>
        ''' <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        ''' <param name="ex">The Exception thrown during data access.</param>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId:="Member")> _
        <EditorBrowsable(EditorBrowsableState.Advanced)> _
        Protected Overridable Sub DataPortal_OnDataPortalException(ByVal e As EventArgs, ByVal ex As Exception)

        End Sub

#End Region

    End Class
End Namespace
