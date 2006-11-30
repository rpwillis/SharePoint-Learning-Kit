/* Copyright (c) Microsoft Corporation. All rights reserved. */


                   RunCS.exe Usage Information

RunCS.exe runs a .cs file from the command line.

To use RunCS.exe, run it as follows:

    RunCS [/reference:<assembly>] <filename> <arg1> <arg2> ... <argn>

<assembly> contains a name of an assembly that is referenced by the code.  Example:
System.Web.dll.  

<filename> is a C# source file.  The file must contain a class with a public static
entry method that accepts an array of string arguments e.g.,:

    namespace MyApp
    {
        class MyClass
        {
            public static int Main(string[] args)
            {
                ...
            }
        }
    }

RunCS.exe loads and compiles the file, and then calls the entry method.  <arg1>..<argn>
are passed as arguments to the entry method.

NOTE: If RunCS ends unexpectedly (e.g., "stop debugging" in a debugger), a DLL and PDB
file may remain in the %TEMP% directory.  These files can be deleted -- they all have
a prefix of "RunCS".
