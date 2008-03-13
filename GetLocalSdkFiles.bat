rem  Copyright (c) Microsoft Corporation. All rights reserved. 

rem -- The sample code in the Samples directory is set up to look for DLLs in
rem -- ..\Debug, i.e. the Debug directory in this directory.  This batch file
rem -- copies Debug and Release subdirectories of the latest build (as well as
rem -- other useful related files) to this directory for testing purposes.

mkdir Debug
mkdir Release

copy /y Src\Compression\Debug\Microsoft.LearningComponents.Compression.dll Debug
copy /y Src\Compression\Debug\Microsoft.LearningComponents.Compression.pdb Debug

copy /y Src\Compression\Release\Microsoft.LearningComponents.Compression.dll Release
copy /y Src\Compression\Release\Microsoft.LearningComponents.Compression.pdb Release

copy /y Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.dll Debug
copy /y Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.pdb Debug

copy /y Src\LearningComponents\bin\Release\Microsoft.LearningComponents.dll Release
copy /y Src\LearningComponents\bin\Release\Microsoft.LearningComponents.pdb Release

copy /y Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.dll Debug
copy /y Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.pdb Debug

copy /y Src\Storage\bin\Release\Microsoft.LearningComponents.Storage.dll Release
copy /y Src\Storage\bin\Release\Microsoft.LearningComponents.Storage.pdb Release

copy /y Src\Schema\InitSchema.sql .

echo Local Files > Debug\_ReadMe.txt

