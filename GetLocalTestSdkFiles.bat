rem  Copyright (c) Microsoft Corporation. All rights reserved. 

rem -- copies test files into the BasicWebPlayer directory

mkdir Debug

copy /y Src\Compression\Debug\Microsoft.LearningComponents.Compression.dll Debug
copy /y Src\Compression\Debug\Microsoft.LearningComponents.Compression.pdb Debug

copy /y Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.dll Debug
copy /y Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.pdb Debug

copy /y Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.dll Debug
copy /y Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.pdb Debug

copy /y Src\SchemaCompiler\bin\Debug\SchemaCompiler.exe Debug
copy /y Src\SchemaCompiler\bin\Debug\SchemaCompiler.pdb Debug

copy /y Src\Schema\InitSchema.sql .

mkdir Samples\BasicWebPlayer\bin
copy /y Debug\* Samples\BasicWebPlayer\bin

del /Q Samples\BasicWebPlayer\bin\*SharePoint*.*

copy /y Src\TestUtilities\LearningStoreHelpers\bin\Debug\LearningStoreHelpers.dll Samples\BasicWebPlayer\bin
copy /y Src\TestUtilities\LearningStoreHelpers\bin\Debug\LearningStoreHelpers.pdb Samples\BasicWebPlayer\bin

echo Local Files > Debug\_ReadMe.txt
