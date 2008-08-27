Imports System.Reflection

'Clase de la cual deben heredar todas aquellas que serán Auditables.  Esto quiere decir
'que cada registro tendrá una fecha de creación, una fecha de última modificación y un 
'usuario que la realizó.  Estos objetos deberán estar asociados a tablas con triggers 
'correspondientes que copien a una tabla de Auditoria el contenido actual de la tabla.
'Estas tablas de auditoría serán movidas en forma regular a otra base de datos para
'no afectar el rendimiento de la base de datos principal.
<Serializable()> _
Public MustInherit Class BLAuditableBase
    Inherits BLBusinessBase

#Region "Business Object Data"
    'Es necesario siempre inicializar los objetos con algun valor
    <FieldMap(False)> Protected m_FechaCreacion As Date = Date.Now
    <FieldMap(False)> Protected m_FechaUltimaModificacion As Date = Date.Now
    <FieldMap(False)> Protected m_UserID As String = ""

#End Region

#Region "Business Properties and Methods"
    Public ReadOnly Property FechaCreacion() As Date
        Get
            Return m_FechaCreacion.Date
        End Get
    End Property

    Public ReadOnly Property FechaUltimaModificacion() As Date
        Get
            Return m_FechaUltimaModificacion.Date
        End Get
    End Property

    Public ReadOnly Property UserID() As String
        Get
            Return m_UserID
        End Get
    End Property


#End Region

#Region "Data Access"
    'Metodo utilizado por DataPortal_Fetch para guardar el objeto en la base de datos
    Public Overrides Sub BLUpdate(Optional ByVal ParentObject As BLBusinessBase = Nothing)

        If Not Me.IsDirty Then Exit Sub

        Try
            If Not Me.IsDeleted Then
                If Me.IsNew Then

                    m_FechaCreacion = Date.Now
                    m_FechaUltimaModificacion = Date.Now
                    m_UserID = System.Threading.Thread.CurrentPrincipal.Identity.Name

                Else
                    m_FechaUltimaModificacion = Date.Now
                    m_UserID = System.Threading.Thread.CurrentPrincipal.Identity.Name
                End If
            End If

        Catch ex As Exception
            Throw New System.Exception(BLResources.Exception_UpdateOperationFailed, ex)
        End Try

        MyBase.BLUpdate(ParentObject)
    End Sub


#End Region

End Class
