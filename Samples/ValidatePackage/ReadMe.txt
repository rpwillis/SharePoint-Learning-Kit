/* Copyright (c) Microsoft Corporation. All rights reserved. */

This sample shows how to use Microsoft Learning Components (MLC) in a
desktop application to validate SCORM 1.2/2004 or Class Server packages.
(MLC is a set of components distributed with SharePoint Learning Kit.)

ValidatePackage.exe has minimal requirements: other than .NET Framework
2.0, this application requires only Microsoft.LearningComponents.dll and
Microsoft.LearningComponents.Compression.dll, and it runs on Windows XP
or above.  The two DLLs can be in the global assembly cache or in the
same directory as ValidatePackage.exe.

Two kinds of errors can be displayed:

  -- SCORM errors: places where the package doesn't conform to the SCORM
     specification.

  -- MLC errors: problems that have the consequence that the package
     cannot be used in SharePoint Learning Kit or applications built on
     SLK's MLC components.

This application can be compiled in Visual Studio 2005.  Alternatively,
run CompileAndRun.bat, which will compile the application using .NET
Framework 2.0 components and then execute it.

If desired, you can specify on the command line a file or directory to
display initially.

