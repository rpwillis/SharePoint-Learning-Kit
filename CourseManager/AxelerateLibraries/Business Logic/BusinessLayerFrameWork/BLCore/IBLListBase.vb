Namespace BLCore

    ''' <summary>
    ''' Interface that contains all the common methods for the different BLListBase templated types
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IBLListBase
        Inherits IBindingList

        Function NewBusinessObject() As BLBusinessBase

        Overloads Function Find(ByVal PropertyName As String, ByVal Value As Object) As BLBusinessBase

        Overloads Function Find(ByVal DKey As BLDataKey) As BLBusinessBase

        Overloads Function Find(ByVal DKey As String) As BLBusinessBase

        Sub BLUpdate(Optional ByVal ParentObject As BLBusinessBase = Nothing, Optional ByVal IndividualSecurity As Boolean = False)

        Sub SecurityCheck(ByVal NivelAcceso As String)

        Sub Local_GetCollection(ByVal Criteria As BLCriteria)

        Property DataLayerContextInfo() As DataLayerContextInfo

        Property DataLayer() As DataLayerAbstraction

        Sub DataPortal_Update()

        Function MarkDirtyRecursive() As Boolean


    End Interface
End Namespace