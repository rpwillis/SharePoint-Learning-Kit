mkdir bin
copy /y ..\..\Src\Compression\Debug\Microsoft.LearningComponents.Compression.dll bin
copy /y ..\..\Src\Compression\Debug\Microsoft.LearningComponents.Compression.pdb bin

copy /y ..\..\Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.dll bin
copy /y ..\..\Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.pdb bin

copy /y ..\..\Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.dll bin
copy /y ..\..\Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.pdb bin

copy /y ..\..\Src\Schema\InitSchema.sql .

del /Q bin\*SharePoint*.*

copy /y ..\..\Src\TestUtilities\LearningStoreHelpers\bin\Debug\LearningStoreHelpers.dll bin
copy /y ..\..\Src\TestUtilities\LearningStoreHelpers\bin\Debug\LearningStoreHelpers.pdb bin

