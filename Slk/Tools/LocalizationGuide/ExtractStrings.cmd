IF NOT EXIST bin mkdir bin
IF NOT EXIST bin\out mkdir bin\out

copy ..\..\..\LearningComponents\LearningComponents\bin\release\Microsoft.LearningComponents.dll bin
copy ..\..\..\LearningComponents\SharePoint\bin\release\Microsoft.LearningComponents.SharePoint.dll bin
copy ..\..\..\LearningComponents\Storage\bin\release\Microsoft.LearningComponents.Storage.dll bin
copy ..\..\dll\bin\release\Microsoft.SharePointLearningKit.dll bin

localize e bin\Microsoft.LearningComponents.dll bin\out\
localize e bin\Microsoft.SharePointLearningKit.dll bin\out\
localize e bin\Microsoft.LearningComponents.SharePoint.dll bin\out\
localize e bin\Microsoft.LearningComponents.Storage.dll bin\out\
