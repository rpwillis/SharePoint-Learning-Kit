rem  Copyright (c) Microsoft Corporation. All rights reserved. 

rem -- see ReadMe.txt
mkdir Bin
copy /y ..\..\Debug\Microsoft.LearningComponents.MRCI.dll Bin
copy /y ..\..\Debug\Microsoft.LearningComponents.MRCI.pdb Bin
copy /y ..\..\Debug\Microsoft.LearningComponents.Compression.dll Bin
copy /y ..\..\Debug\Microsoft.LearningComponents.Compression.pdb Bin
copy /y ..\..\Debug\Microsoft.LearningComponents.dll Bin
copy /y ..\..\Debug\Microsoft.LearningComponents.pdb Bin
copy /y ..\..\Debug\Microsoft.LearningComponents.Storage.dll Bin
copy /y ..\..\Debug\Microsoft.LearningComponents.Storage.pdb Bin
call CompileSchema.bat
