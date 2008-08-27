Namespace BLCore.DataTypes

    ''' <summary>
    ''' Utility class that declares a type of application
    ''' </summary>
    ''' <typeparam name="FirstType"></typeparam>
    ''' <typeparam name="SecondType"></typeparam>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class Pair(Of FirstType, SecondType)
        Implements IPair

        ''' <summary>
        ''' Reference to the first object
        ''' </summary>
        ''' <remarks></remarks>
        Private m_First As FirstType

        ''' <summary>
        ''' Reference to the second object
        ''' </summary>
        ''' <remarks></remarks>
        Private m_Second As SecondType

        Public Sub New(ByVal nFirst As FirstType, ByVal nSecond As SecondType)
            m_First = nFirst
            m_Second = nSecond
        End Sub

        Public Property First() As FirstType
            Get
                Return m_First
            End Get
            Set(ByVal value As FirstType)
                m_First = value
            End Set
        End Property

        Public Property Second() As SecondType
            Get
                Return m_Second
            End Get
            Set(ByVal value As SecondType)
                m_Second = value
            End Set
        End Property

        Private Property FirstObject() As Object Implements IPair.First
            Get
                Return First
            End Get
            Set(ByVal value As Object)
                First = CType(value, FirstType)
            End Set
        End Property

        Private Property SecondObject() As Object Implements IPair.Second
            Get
                Return Second
            End Get
            Set(ByVal value As Object)
                Second = CType(value, SecondType)
            End Set
        End Property

        Public Shared Function FindPair(ByVal PairList() As Pair(Of FirstType, SecondType), ByVal First As FirstType) As Pair(Of FirstType, SecondType)
            Dim i As Integer = 0
            While i < PairList.Length
                If PairList(i).First.Equals(First) Then
                    Return PairList(i)
                End If
                i = i + 1
            End While
            Return Nothing
        End Function

        Public Shared Function FindSecond(ByVal PairList() As Pair(Of FirstType, SecondType), ByVal First As FirstType) As SecondType
            Dim Pair As Pair(Of FirstType, SecondType) = FindPair(PairList, First)
            If Not Pair Is Nothing Then
                Return Pair.Second
            Else
                Return Nothing
            End If
        End Function

    End Class

    Public Interface IPair
        Property First() As Object
        Property Second() As Object
    End Interface

End Namespace