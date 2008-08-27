Imports System.Reflection

Namespace BLCore

    Public Enum FactoryMethodType
        FMTNew
        FMTGet
    End Enum

    Public Class ReflectionHelper

        Private Shared Function GetFactoryMethodName(ByVal BusinessType As Type, ByVal FactoryMethod As FactoryMethodType) As String
            Select Case FactoryMethod
                Case FactoryMethodType.FMTGet
                    If InheritsFrom(GetType(BLBusinessBase), BusinessType) Then
                        Return "GetObject"
                    Else
                        Return "GetCollection"
                    End If

                Case FactoryMethodType.FMTNew
                    If InheritsFrom(GetType(BLBusinessBase), BusinessType) Then
                        Return "NewObject"
                    Else
                        Return "NewCollection"
                    End If
            End Select
            Return ""
        End Function

        Public Shared Function InvokeBusinessFactoryMethod(ByVal ClassName As String, ByVal FactoryMethod As FactoryMethodType, _
            Optional ByVal Args() As Object = Nothing) As Object

            Dim MyType As System.Type = CreateBusinessType(ClassName)
            Dim MethodName As String = GetFactoryMethodName(MyType, FactoryMethod)

            If Not Args Is Nothing Then
                Return MyType.InvokeMember(MethodName, _
                    BindingFlags.FlattenHierarchy Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static Or BindingFlags.InvokeMethod Or Reflection.BindingFlags.Default, _
                    Nothing, Nothing, Args)
            Else
                Return MyType.InvokeMember(MethodName, _
                    BindingFlags.FlattenHierarchy Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static Or BindingFlags.InvokeMethod Or Reflection.BindingFlags.Default, _
                    Nothing, Nothing, Nothing)
            End If
        End Function

        Public Shared Function InvokeSharedBusinessClassMethod(ByVal ClassName As String, ByVal MethodName As String, _
            Optional ByVal Args() As Object = Nothing) As Object

            Dim MyType As System.Type = CreateBusinessType(ClassName)

            If Not Args Is Nothing Then
                Return MyType.InvokeMember(MethodName, _
                    BindingFlags.DeclaredOnly Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static Or BindingFlags.InvokeMethod Or Reflection.BindingFlags.Default, _
                    Nothing, Nothing, Args)
            Else
                Return MyType.InvokeMember(MethodName, _
                    BindingFlags.DeclaredOnly Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static Or BindingFlags.InvokeMethod Or Reflection.BindingFlags.Default, _
                    Nothing, Nothing, Nothing)
            End If


        End Function

        Public Shared Function GetSharedBusinessClassProperty(ByVal ClassName As String, ByVal MethodName As String, _
            Optional ByVal Args() As Object = Nothing) As Object
            Dim MyType As System.Type
            Try
                MyType = CreateBusinessType(ClassName)
                If Not Args Is Nothing Then
                    'BindingFlags.DeclaredOnly was removed by daniel, and added a BindingFlags.FlattenHierarchy and a BindingFlags.InvokeMethod
                    Return MyType.InvokeMember(MethodName, _
                         BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static Or BindingFlags.GetProperty Or Reflection.BindingFlags.Default Or BindingFlags.FlattenHierarchy Or BindingFlags.InvokeMethod, _
                        Nothing, Nothing, Args)
                Else
                    Return MyType.InvokeMember(MethodName, _
                         BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static Or BindingFlags.GetProperty Or Reflection.BindingFlags.Default Or BindingFlags.FlattenHierarchy Or BindingFlags.InvokeMethod, _
                        Nothing, Nothing, Nothing)
                End If
            Catch e As System.Exception
                MyType = Nothing
            End Try

            Return Nothing
        End Function

        Public Shared Function IsSameClass(ByVal Class1Name As String, ByVal Class2Name As String) As Boolean
            Return CreateBusinessType(Class1Name).FullName = CreateBusinessType(Class2Name).FullName
        End Function


        Friend Shared Function CreateBusinessObject(ByVal ClassName As String, Optional ByVal Args() As Object = Nothing) As Object
            Dim MyType As System.Type = CreateBusinessType(ClassName)
            If Not Args Is Nothing Then
                Return System.Activator.CreateInstance(MyType, Args)
            Else
                Return System.Activator.CreateInstance(MyType)
            End If
        End Function

        Public Shared Function CreateBusinessType(ByVal TypeName As String) As Type
            Return System.Type.GetType(TypeName, True, False)
        End Function


        Public Shared Function GetClassName(ByVal ClassName As String) As String
            Dim DotIndex As Integer = ClassName.LastIndexOf(".")
            If (DotIndex > 0) Then
                Return ClassName.Substring(DotIndex + 1)
            Else
                Return ClassName
            End If
        End Function


        Public Shared Function CreateInterfaceObject(ByVal ClassName As String, Optional ByVal Args() As Object = Nothing) As Object

            Dim MyType As System.Type = CreateBusinessType(ClassName)
            If Not MyType Is Nothing Then
                If Args Is Nothing Then
                    Return System.Activator.CreateInstance(MyType)
                Else
                    Return System.Activator.CreateInstance(MyType, Args)
                End If
            Else
                Debug.WriteLine(My.Resources.BLResources.createInterfaceObjectIsNothing)
                Return Nothing
            End If
        End Function

        Public Shared Function InheritsFrom(ByVal ParentType As System.Type, ByVal InheritedType As System.Type) As Boolean
            Return ParentType.IsAssignableFrom(InheritedType)

            'Dim ToReturn As Boolean = False
            'Dim SearchType As System.Type = InheritedType
            'While Not ToReturn And Not SearchType Is Nothing
            'If SearchType Is ParentType Then
            'ToReturn = True
            'Else
            'SearchType = SearchType.BaseType
            'End If

            'End While


            'Return ToReturn
        End Function

        ''' <summary>
        ''' Takes a business object or a collection of business objects and returns
        ''' the type of the business object associated
        ''' </summary>
        ''' <param name="BusinessType"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared ReadOnly Property ResolveBusinessType(ByVal BusinessType As Type) As Type
            Get
                Try
                    If InheritsFrom(GetType(BLBusinessBase), BusinessType) Then
                        Return BusinessType
                    Else
                        If BusinessType IsNot Nothing Then
                            While Not BusinessType.Name = GetType(BLListBase(Of ,,)).Name
                                BusinessType = BusinessType.BaseType
                            End While

                            Dim Arguments() As Type = BusinessType.GetGenericArguments()
                            Return Arguments(1)
                        Else
                            Return Nothing
                        End If
                    End If
                    Return Nothing
                Catch ex As System.Exception
                End Try
                Return Nothing
            End Get
        End Property


    End Class
End Namespace