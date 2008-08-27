Namespace DLCore

    ''' <summary>
    ''' Class that represnts a parameter of data to pass to the DataLayer for any functions of himself
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DataLayerParameter

        Private m_Name As String
        Private m_Value As Object

        Public Sub New(ByVal NName As String, ByVal NValue As Object)
            Name = NName
            Value = NValue
        End Sub

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Public Property Value() As Object
            Get
                Return m_Value
            End Get
            Set(ByVal value As Object)
                m_Value = value
            End Set
        End Property

#Region "SQLServer Dependant Code"
        'In case this parameters involve a SQL parameter, returns the regarding
        'SQLParameter object. Could specify the name of the parameter, if not
        'uses the name of the field in the database
        Public ReadOnly Property GetSQLParameter() As SqlParameter
            Get
                'Return New SqlParameter("@Filter_" + m_Name, m_Value)
                Return New SqlParameter(m_Name, m_Value)
            End Get
        End Property

#End Region

#Region "OLEDB Dependant Code"
        'In case this parameters involve a OLEDB parameter, returns the regarding
        'OLEDBParameter object. Could specify the name of the parameter, if not
        'uses the name of the field in the database
        Public ReadOnly Property GetOLEDBParameter() As OleDb.OleDbParameter
            Get
                Return New OleDb.OleDbParameter(m_Name, m_Value)
            End Get
        End Property

#End Region


    End Class
End Namespace
