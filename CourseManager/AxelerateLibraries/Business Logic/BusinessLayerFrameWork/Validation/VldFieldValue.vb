Namespace BLCore.Validation
    <Serializable()> _
    Public Class VldFieldValue

#Region "Class Level Private Variables "
        Private m_BusinessObject As BLBusinessBase
        Private m_TestCriteria As BLCriteria

        Private m_isValid As Boolean

#End Region

#Region " Constructors "

        Private Sub New()
            ' prevent direct creation
        End Sub
#End Region

#Region "ReadOnlyBase Overrides"
        Protected Function GetIdValue() As Object
            Return Me
        End Function
#End Region

#Region " Business Properties and Methods "
        Public ReadOnly Property isValid() As Boolean
            Get
                Return m_isValid
            End Get
        End Property
#End Region

#Region " Shared Methods "

        Public Shared Function GetVldFieldValue(ByVal BusinessObject As BLBusinessBase, _
            ByVal TestCriteria As BLCriteria) As VldFieldValue
            'Load an Existing Object from DB
            ' TODO: Not possible casting from Criteria to VldDuplicate
            'Return CType(DataPortal.Fetch(New Criteria(BusinessObject, TestCriteria)), VldFieldValue)
            Throw New NotSupportedException(My.Resources.BLResources.duplicateValueNotSupported)
        End Function
#End Region

#Region " Criteria Class "

        ' criteria for identifying existing object and is always serializable
        <Serializable()> _
        Private Class Criteria
            Public m_BusinessObject As BLBusinessBase
            Public m_TestCriteria As BLCriteria

            Public Sub New(ByVal BusinessObject As BLBusinessBase, ByVal TestCriteria As BLCriteria)
                m_BusinessObject = BusinessObject
                m_TestCriteria = TestCriteria
            End Sub

        End Class

#End Region ' Criteria

#Region "DataPortal Overrides"

        ' called by DataPortal to load data from the database
        Protected Sub DataPortal_Fetch(ByVal Criteria As Object)
            Dim crit As Criteria = CType(Criteria, Criteria)

            Try
                Dim DataLayer As DataLayerAbstraction = crit.m_BusinessObject.DataLayer
                Dim ReadCommand As DataLayerCommandBase = DataLayer.ReadCommand( _
                    crit.m_BusinessObject.DataLayer, crit.m_TestCriteria)
                Try
                    ReadCommand.Execute()

                    m_isValid = False
                    If ReadCommand.NextRecord Then
                        m_isValid = True
                    End If
                Catch ex As System.Exception
                    Throw ex
                Finally
                    ReadCommand.Finish()
                End Try




            Catch ex As Exception
                Throw New Exception(BLResources.Exception_FetchOperationFailed, ex)
            End Try


        End Sub

#End Region ' Data Access 

    End Class
End Namespace