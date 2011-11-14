rem  Copyright (c) Microsoft Corporation. All rights reserved. 

rem -- Compile the program...
mkdir bin
mkdir bin\Debug
%windir%\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:winexe /out:bin\Debug\ValidatePackage.exe /r:..\..\Debug\Microsoft.LearningComponents.dll  ValidatePackage.cs Program.cs ValidatePackage.Designer.cs Properties\AssemblyInfo.cs Properties\Resources.Designer.cs Properties\Settings.Designer.cs

if errorlevel 1 goto error

rem -- Run the program...
start bin\Debug\ValidatePackage.exe
goto exit

:error
pause
:exit

