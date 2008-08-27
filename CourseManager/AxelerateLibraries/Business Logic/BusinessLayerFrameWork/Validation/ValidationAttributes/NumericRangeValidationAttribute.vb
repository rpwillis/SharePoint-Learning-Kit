Namespace BLCore.Validation.ValidationAttributes
    ''' <summary>
    ''' Verifica que la propiedad sea numérica y que esté en el rango válido
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Property)> _
    Public Class NumericRangeValidationAttribute
        Inherits SinglePropertyValidationAttribute
        Private m_MinValue As Double = 0
        Private m_MaxValue As Double = 0


        Public Sub New(Optional ByVal MinValue As Double = Double.MinValue, Optional ByVal MaxValue As Double = Double.MaxValue)
            MyBase.New()
            m_MinValue = MinValue
            m_MaxValue = MaxValue
        End Sub

#Region "ValidationAttribute Overrides"
        Public Overrides Function Validate(ByVal target As Object, ByRef e As String) As Boolean
            Dim BLTarget As BLBusinessBase = CType(target, BLBusinessBase)
            Dim Value As Object = ReflectedProperty(BLTarget)
            If (IsNumeric(Value)) Then
                Dim DBLValue As Double = CDbl(Value)
                If DBLValue >= m_MinValue And DBLValue <= m_MaxValue Then
                    Return True
                End If
            End If
            e = ValidationFailedMessage
            Return False
        End Function
#End Region

#Region "SinglePropertyValidationAttribute Overrides"
        Protected Overrides ReadOnly Property ValidationFailedMessage() As String
            Get
                Return My.Resources.BLResources.Validation_DuplicateKey.Replace("[PropertyName]", PropertyName)
            End Get
        End Property
#End Region

    End Class
End Namespace
