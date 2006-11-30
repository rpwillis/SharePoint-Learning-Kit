rem  Copyright (c) Microsoft Corporation. All rights reserved. 

rem  This batch file should *NOT* be used unless you have an unsigned build.

rem -- see ReadMe.txt
for %%f in (bin\*.dll) do sn -Vr %%f
sn -Vr ..\..\Debug\SchemaCompiler.exe
