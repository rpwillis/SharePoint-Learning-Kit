Namespace DLCore
    Public Class TestException
        Inherits Exception
        Public TestMessage As String = ""
    End Class
    Public Class FieldCreationTestException
        Inherits TestException
        Public FieldList As BLFieldMapList = Nothing
    End Class
    Public Class TableCreationTestException
        Inherits TestException
        Public TableName As String = ""
    End Class
    Public Class DataBaseConnectionTestException
        Inherits TestException

    End Class
    Public Class LogicalCreationTestException
        Inherits TestException

    End Class
    Public Class RelateObjectCreationTestException
        Inherits TestException

    End Class
End Namespace