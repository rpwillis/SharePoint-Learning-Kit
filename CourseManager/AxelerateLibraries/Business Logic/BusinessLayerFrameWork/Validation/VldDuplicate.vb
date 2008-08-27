Namespace BLCore.Validation

    <Serializable()> _
    Public Class VldDuplicate

#Region "ReadOnlyBase Overrides"
        Protected Function GetIdValue() As Object
            Return Me
        End Function
#End Region

#Region "Class Level Private Variables "
        Private m_isValid As Boolean

#End Region

#Region " Constructors "

        Private Sub New()
            ' prevent direct creation
        End Sub
#End Region

#Region " Business Properties and Methods "
        Public ReadOnly Property isValid() As Boolean
            Get
                Return m_isValid
            End Get
        End Property
#End Region

#Region " Shared Methods "

        Public Shared Function GetVldDuplicate(ByVal BusinessObject As BLBusinessBase) As VldDuplicate
            'Load an Existing Object from DB
            ' TODO: Not possible casting from Criteria to VldDuplicate
            'Return CType(DataPortal.Fetch(New Criteria(BusinessObject)), VldDuplicate)
            Throw New NotSupportedException("Duplicate value not supported")
        End Function
#End Region

#Region " Criteria Class "

        ' criteria for identifying existing object and is always serializable
        <Serializable()> _
        Private Class Criteria
            Public m_BusinessObject As BLBusinessBase

            Public Sub New(ByVal BusinessObject As BLBusinessBase)
                m_BusinessObject = BusinessObject
            End Sub

        End Class

#End Region ' Criteria

#Region "DataPortal Overrides"

        ' called by DataPortal to load data from the database
        Protected Sub DataPortal_Fetch(ByVal Criteria As Object)
            Dim crit As Criteria = CType(Criteria, Criteria)

            Try

                Dim ReadCommand As DataLayerCommandBase
                Dim DataLayer As DataLayerAbstraction = crit.m_BusinessObject.DataLayer
                ReadCommand = DataLayer.ReadCommand(crit.m_BusinessObject, crit.m_BusinessObject.KeyCriteria)
                Try
                    ReadCommand.Execute()
                    m_isValid = True
                    If ReadCommand.NextRecord Then
                        m_isValid = False
                    End If
                Catch ex As Exception
                    Throw ex
                Finally
                    ReadCommand.Finish()
                End Try
            Catch ex As Exception
                Throw New System.Exception(BLResources.Exception_FetchOperationFailed, ex)
            End Try


        End Sub

#End Region ' Data Access 

    End Class
End Namespace
