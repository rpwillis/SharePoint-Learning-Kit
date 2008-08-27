Imports System.Threading
Public Class DataTableInformation
    Protected m_Name As String = ""
    Protected m_Lock As New ReaderWriterLock
    Protected m_AutonumericValues As Hashtable = Nothing
    'Protected m_NeedAutonumericUpdate As Boolean = True
    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(ByVal value As String)
            m_Name = value
        End Set
    End Property
    Public Property Lock() As ReaderWriterLock
        Get
            Return m_Lock
        End Get
        Set(ByVal value As ReaderWriterLock)
            m_Lock = value
        End Set
    End Property
    Public Property AutonumericValues() As Hashtable
        Get
            Return m_AutonumericValues
        End Get
        Set(ByVal value As Hashtable)
            m_AutonumericValues = value
        End Set
    End Property

    'Public Property NeedAutonumericUpdate() As Boolean
    '    Get
    '        Return m_NeedAutonumericUpdate
    '    End Get
    '    Set(ByVal value As Boolean)
    '        m_NeedAutonumericUpdate = value
    '    End Set
    'End Property

    Public Sub New(ByVal DataTableName As String)
        m_Name = DataTableName
        'm_AutonumericValues = New Collection
    End Sub
End Class
