Imports Microsoft.VisualBasic

Namespace BLCore.Templates
    <Serializable()> _
    Public MustInherit Class GUIDNameBusinessTemplate( _
        Of tBusinessObject As {GUIDNameBusinessTemplate(Of tBusinessObject), New})
        Inherits GUIDTemplate(Of tBusinessObject)

#Region "Business Object Data"
        <FieldMap(False)> Protected m_Name As String = ""
#End Region

#Region "Business Properties and Methods"

        <ValidationAttributes.StringRequiredValidation(), ValidationAttributes.StringLengthValidation(50)> _
        Public Property Name() As String
            Get
                Return m_Name

            End Get
            Set(ByVal value As String)
                m_Name = value
                PropertyHasChanged()
            End Set
        End Property
#End Region


#Region "System.Object Overrides"

        'Se redefine el metodo ToString para que devuelva el identificador único (Criteria) del objeto
        Public Overrides Function ToString() As String
            Return m_Name
        End Function
#End Region

    End Class
End Namespace
