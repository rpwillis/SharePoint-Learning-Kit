Namespace BLCore.Templates

    '
    ''' <summary>
    ''' Defines the factory methods and the attribute validation
    ''' </summary>
    ''' <typeparam name="tBusinessObject"></typeparam>
    ''' <typeparam name="tDataKey"></typeparam>
    ''' <remarks></remarks>
    <Serializable()> _
    Public MustInherit Class BLBusinessBase(Of tBusinessObject As {BLBusinessBase(Of tBusinessObject, tDataKey), New}, tDataKey As BLDataKey)
        Inherits BLCore.BLBusinessBase

#Region "Factory"
        Public Class BusinessFactory(Of tDataLayerType As DataLayerAbstraction)
            ''' <summary>
            ''' Returns a new bussines object empty
            ''' </summary>
            ''' <param name="NDataLayerContextInfo"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <staFactory()> _
            Public Shared Function NewObject(Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tBusinessObject
                Dim BO As tBusinessObject = New tBusinessObject
                If Not NDataLayerContextInfo Is Nothing Then
                    BO.DataLayerContextInfo.Copy(NDataLayerContextInfo, BO.GetType)
                    If BO.DataLayerContextInfo.DataLayerType Is Nothing Then
                        BO.DataLayerContextInfo.DataLayerType = GetType(tDataLayerType)
                    End If
                End If
                Return BO
            End Function

            ''' <summary>
            ''' Function called by all the other GetObject for returning an object of the bussines layer based on Criteria
            ''' </summary>
            ''' <param name="Criteria"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <staFactory()> _
            Public Shared Function GetObject(ByVal Criteria As BusinessObjectCriteria) As tBusinessObject
                Dim BO As tBusinessObject

                If Criteria.DataLayerContextInfo.DataLayerType Is Nothing Then
                    Criteria.DataLayerContextInfo.DataLayerType = GetType(tDataLayerType)
                End If

                BO = New tBusinessObject
                BO.DataLayerContextInfo.Copy(Criteria.DataLayerContextInfo, GetType(tBusinessObject))
                BO.Local_GetObject(Criteria)
                Return BO
            End Function

            ''' <summary>
            ''' Returns a new bussines object empty, that is read from the database using his key
            ''' </summary>
            ''' <param name="DataKey"></param>
            ''' <param name="NDataLayerContextInfo"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <staFactory()> _
            Public Shared Function GetObject(ByVal DataKey As tDataKey, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tBusinessObject
                Dim Crit As BusinessObjectCriteria = New BusinessObjectCriteria(DataKey)
                Crit.DataLayerContextInfo = NDataLayerContextInfo
                Return GetObject(Crit)
            End Function

            ''' <summary>
            ''' Returns a new bussines object empty, that is read from the database using his key
            ''' </summary>
            ''' <param name="DataKey"></param>
            ''' <param name="NDataLayerContextInfo"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <staFactory()> _
            Public Shared Function GetObject(ByVal DataKey As BLDataKey, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tBusinessObject
                If DataKey.GetType Is GetType(tDataKey) Then
                    Dim Crit As BusinessObjectCriteria = New BusinessObjectCriteria(CType(DataKey, tDataKey))
                    Crit.DataLayerContextInfo = NDataLayerContextInfo
                    Return GetObject(Crit)
                Else
                    Throw New Exception(BLResources.Exception_DataKeyMismatch)
                End If
            End Function

            ''' <summary>
            ''' Returns a bussines object based on a Criteria
            ''' </summary>
            ''' <param name="Criteria"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <staFactory()> _
            Public Shared Function GetObject(ByVal Criteria As BLCriteria) As tBusinessObject
                Dim Crit As BusinessObjectCriteria = New BusinessObjectCriteria(Criteria)
                Return GetObject(Crit)
            End Function


        End Class

        ''' <summary>
        ''' Returns a new bussines object empty
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <staFactory()> _
        Public Shared Function NewObject() As tBusinessObject
            Dim BO As New tBusinessObject
            Return BO
        End Function


        ''' <summary>
        ''' Returns a new bussines object empty
        ''' </summary>
        ''' <param name="NDataLayerContextInfo"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <staFactory()> _
        Public Shared Function NewObject(ByVal NDataLayerContextInfo As DataLayerContextInfo) As tBusinessObject
            Dim BO As New tBusinessObject
            BO.DataLayerContextInfo = NDataLayerContextInfo
            Return BO
        End Function

        ''' <summary>
        ''' Function called by all the other GetObject for returning an object of the bussines layer based on Criteria
        ''' </summary>
        ''' <param name="Criteria"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <staFactory()> _
        Public Shared Function GetObject(ByVal Criteria As BusinessObjectCriteria) As tBusinessObject
            Dim BO As tBusinessObject

            BO = New tBusinessObject
            BO.DataLayerContextInfo.Copy(Criteria.DataLayerContextInfo, GetType(tBusinessObject))
            BO.Local_GetObject(Criteria)
            Return BO
        End Function


        ' Returns a new bussines object empty, that is read from the database using his key
        <staFactory()> _
        Public Shared Function GetObject(ByVal DataKey As tDataKey, ByVal NDataLayerContextInfo As DataLayerContextInfo) As tBusinessObject
            Dim Crit As New BusinessObjectCriteria(DataKey)
            Crit.DataLayerContextInfo = NDataLayerContextInfo
            Return GetObject(Crit)
        End Function

        ' Returns a new bussines object empty, that is read from the database using his key
        <staFactory()> _
        Public Shared Function GetObject(ByVal DataKey As tDataKey) As tBusinessObject
            Dim Crit As New BusinessObjectCriteria(DataKey)
            Return GetObject(Crit)
        End Function
        <staFactory()> _
        Public Shared Function GetObject(ByVal DataKey As BLDataKey, Optional ByVal NDataLayerContextInfo As DataLayerContextInfo = Nothing) As tBusinessObject
            If DataKey.GetType Is GetType(tDataKey) Then
                Dim Crit As New BusinessObjectCriteria(CType(DataKey, tDataKey))
                Crit.DataLayerContextInfo = NDataLayerContextInfo
                Return GetObject(Crit)
            Else
                Throw New Exception(BLResources.Exception_DataKeyMismatch)
            End If
        End Function

        'Returns a bussines object based on a Criteria
        <staFactory()> _
        Public Shared Function GetObject(ByVal Criteria As BLCriteria) As tBusinessObject
            Dim Crit As New BusinessObjectCriteria(Criteria)
            Return GetObject(Crit)
        End Function

        'Returns a bussines object based on a Criteria. If doesn't exists returns Nothing (null in c#)
        <staFactory()> _
        Public Shared Function TryGetObject(ByVal Criteria As BLCriteria) As tBusinessObject
            'Dim Crit As New BusinessObjectCriteria(Criteria)
            'Crit.TryGet = True
            'Dim result As tBusinessObject
            'result = GetObject(Crit)
            'If result.DataKey.isEmpty Then
            '    Return Nothing
            'End If
            'Return result
            Try
                Return GetObject(Criteria)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        'Returns the first object found
        <staFactory()> _
        Public Shared Function GetAnyObject(ByVal ContextInfo As DataLayerContextInfo) As tBusinessObject
            Dim Criteria As BLCriteria = New BLCriteria
            Criteria.DataLayerContextInfo = ContextInfo
            Criteria.MaxRegisters = 1
            Return GetObject(Criteria)
        End Function


#End Region

#Region "Criteria Class"
        'Redefines the class AutoMapCriteria in roder to identify the new bussines class
        <Serializable()> _
        Public Class BusinessObjectCriteria
            Inherits BLCore.BLCriteria

            Public Sub New(ByVal Criteria As BLCriteria)
                MyBase.New(Criteria, GetType(tBusinessObject))
            End Sub

            'Builds a new Criteria that contains the objects key
            Public Sub New(ByVal DataKey As tDataKey)
                MyBase.New(GetType(tBusinessObject))
                DataKey.AddToCriteria(Me)
            End Sub

            'Builds a new Criteria empty
            Public Sub New()
                MyBase.New(GetType(tBusinessObject))
            End Sub
        End Class


        'Returns a new Criteria that contains the objects key
        Public Overrides ReadOnly Property KeyCriteria() As BLCore.BLCriteria
            Get
                Return New BusinessObjectCriteria(CType(Me.DataKey, tDataKey))
            End Get
        End Property

        'Returns a new Criteria that contains the objects key
        Public ReadOnly Property TypedDataKey() As tDataKey
            Get
                Return CType(Me.DataKey, tDataKey)
            End Get
        End Property

#End Region

#Region " Clone "

        ''' <summary>
        ''' Creates a clone of the object.
        ''' </summary>
        ''' <returns>
        ''' A new object containing the exact data of the original object.
        ''' </returns>
        Public Overridable Function Clone() As tBusinessObject

            Return DirectCast(GetClone(), tBusinessObject)

        End Function

#End Region

#Region " Data Access "

        ''' <summary>
        ''' Saves the object to the database.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Calling this method starts the save operation, causing the object
        ''' to be inserted, updated or deleted within the database based on the
        ''' object's current state.
        ''' </para><para>
        ''' If <see cref="Core.BusinessBase.IsDeleted" /> is <see langword="true"/>
        ''' the object will be deleted. Otherwise, if <see cref="Core.BusinessBase.IsNew" /> 
        ''' is <see langword="true"/> the object will be inserted. 
        ''' Otherwise the object's data will be updated in the database.
        ''' </para><para>
        ''' All this is contingent on <see cref="Core.BusinessBase.IsDirty" />. If
        ''' this value is <see langword="false"/>, no data operation occurs. 
        ''' It is also contingent on <see cref="Core.BusinessBase.IsValid" />. 
        ''' If this value is <see langword="false"/> an
        ''' exception will be thrown to indicate that the UI attempted to save an
        ''' invalid object.
        ''' </para><para>
        ''' It is important to note that this method returns a new version of the
        ''' business object that contains any data updated during the save operation.
        ''' You MUST update all object references to use this new version of the
        ''' business object in order to have access to the correct object data.
        ''' </para><para>
        ''' You can override this method to add your own custom behaviors to the save
        ''' operation. For instance, you may add some security checks to make sure
        ''' the user can save the object. If all security checks pass, you would then
        ''' invoke the base Save method via <c>MyBase.Save()</c>.
        ''' </para>
        ''' </remarks>
        ''' <returns>A new object containing the saved values.</returns>
        Public Overridable Shadows Function Save() As tBusinessObject
            Return CType(MyBase.Save, tBusinessObject)
        End Function

        ''' <summary>
        ''' Saves the object to the database, forcing
        ''' IsNew to <see langword="false"/> and IsDirty to True.
        ''' </summary>
        ''' <param name="forceUpdate">
        ''' If <see langword="true"/>, triggers overriding IsNew and IsDirty. 
        ''' If <see langword="false"/> then it is the same as calling Save().
        ''' </param>
        ''' <returns>A new object containing the saved values.</returns>
        ''' <remarks>
        ''' This overload is designed for use in web applications
        ''' when implementing the Update method in your 
        ''' data wrapper object.
        ''' </remarks>
        Public Overridable Shadows Function Save(ByVal forceUpdate As Boolean) As tBusinessObject
            Return CType(MyBase.Save(forceUpdate), tBusinessObject)
        End Function


#End Region

#Region " Validation Rules "
        Private Shared m_ValidationRules As ValidationRules = Nothing

        Shared Sub CreateValidationRules()
            m_ValidationRules = New ValidationRules
            For Each PropertyInfo As Reflection.PropertyInfo In GetType(tBusinessObject).GetProperties(Reflection.BindingFlags.FlattenHierarchy Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Public)
                Dim Attributes() As Object = PropertyInfo.GetCustomAttributes(True)
                If Attributes.Length > 0 Then
                    For Each AttributeInstance As Attribute In Attributes
                        If AttributeInstance.GetType.IsSubclassOf(GetType(SinglePropertyValidationAttribute)) Then

                            Dim ValidationAttributeInstance As SinglePropertyValidationAttribute = _
                                CType(AttributeInstance, SinglePropertyValidationAttribute)

                            If (ValidationAttributeInstance.PropertyName = "") Then
                                ValidationAttributeInstance.PropertyName = PropertyInfo.Name
                                ValidationAttributeInstance.PropertyDescription = PropertyInfo.Name + " property"
                            End If
                            m_ValidationRules.Add( _
                            ValidationAttributeInstance.[GetType]().Name + "_" + PropertyInfo.Name, _
                            ValidationAttributeInstance)
                        End If
                    Next
                End If
            Next
        End Sub

        ''' <summary>
        ''' Provides access to the validation rules collection
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Overrides ReadOnly Property ValidationRules() As ValidationRules
            Get
                If m_ValidationRules Is Nothing Then
                    CreateValidationRules()
                End If
                Return m_ValidationRules
            End Get
        End Property

#End Region

    End Class
End Namespace