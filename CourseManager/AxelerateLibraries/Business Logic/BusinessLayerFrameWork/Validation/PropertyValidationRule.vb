Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace BLCore.Validation
    ''' <summary>
    ''' Base class for propert validation
    ''' </summary>
    <Serializable()> _
    Public MustInherit Class PropertyValidationRule
        Inherits ValidationRule

        ''' <summary>
        ''' Name of the property to be validated
        ''' </summary>
        Private m_PropertyName As String

        ''' <summary>
        ''' Description of the property to be validated
        ''' </summary>
        Private m_PropertyDescription As String

        ''' <summary>
        ''' Initializes a new instance of the class with a specified PropertyName and PropertyDescription
        ''' </summary>
        ''' <param name="pPropertyName">Name of the Property </param>
        ''' <param name="pPropertyDescription">Description of the Property</param>
        Public Sub New(ByVal pPropertyName As String, ByVal pPropertyDescription As String)
            m_PropertyName = pPropertyName
            m_PropertyDescription = pPropertyDescription
        End Sub

        ''' <summary>
        ''' Gets and sets the name of the property
        ''' </summary>
        Public Property PropertyName() As String
            Get
                Return m_PropertyName
            End Get
            Set(ByVal value As String)
                m_PropertyName = value
            End Set
        End Property

        ''' <summary>
        ''' Gets and sets the desciption of the property
        ''' </summary>
        Public Property PropertyDescription() As String
            Get
                Return m_PropertyDescription
            End Get
            Set(ByVal value As String)
                m_PropertyDescription = value
            End Set
        End Property

        ''' <summary>
        ''' Uses reflection on the RuleTarget object to get the property value
        ''' </summary>
        ''' <param name="RuleTarget">Instance of the provisioning object used to obtain the value of the property</param>
        ''' <returns></returns>
        Public Function GetPropertyValue(ByVal RuleTarget As BLBusinessBase) As Object
            Return RuleTarget.GetPropertyValue(PropertyName)
        End Function

        ''' <summary>
        ''' Uses reflection on the RuleTarget object to get the property value
        ''' </summary>
        ''' <param name="RuleTarget">Instance of the provisioning object used to set the value of the property</param>
        ''' <param name="Value">Value for the property to be set</param>
        Public Sub SetPropertyValue(ByVal RuleTarget As BLBusinessBase, ByVal Value As Object)
            RuleTarget.SetPropertyValue(PropertyName, Value)
        End Sub
    End Class
End Namespace
