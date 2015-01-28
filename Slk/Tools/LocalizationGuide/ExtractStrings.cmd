IF NOT EXIST bin mkdir bin
IF NOT EXIST bin\out mkdir bin\out

REM Copy to source files to the bin folder to work with
copy ..\..\..\LearningComponents\LearningComponents\bin\v3.5\Release\Microsoft.LearningComponents.dll bin
copy ..\..\..\LearningComponents\Storage\bin\v3.5\Release\Microsoft.LearningComponents.Storage.dll bin
copy ..\..\..\LearningComponents\SharePoint\bin\2010\Release\Microsoft.LearningComponents.SharePoint.dll bin
copy ..\..\dll\bin\2010\release\Microsoft.SharePointLearningKit.dll bin

REM Extract the resources into the bin\out folder into xml format
localize e bin\Microsoft.LearningComponents.dll bin\out\
localize e bin\Microsoft.SharePointLearningKit.dll bin\out\
localize e bin\Microsoft.LearningComponents.SharePoint.dll bin\out\
localize e bin\Microsoft.LearningComponents.Storage.dll bin\out\
