@echo off
rem  Copyright (c) Microsoft Corporation. All rights reserved. 
rem -- deletes MLC and SLK DLLs from the Global Assembly Cache
gacutil /nologo /u Microsoft.LearningComponents.MRCI > nul
gacutil /nologo /u Microsoft.LearningComponents.Compression > nul
gacutil /nologo /u Microsoft.LearningComponents > nul
gacutil /nologo /u Microsoft.LearningComponents.SharePoint > nul
gacutil /nologo /u Microsoft.LearningComponents.Storage > nul
gacutil /nologo /u Microsoft.SharePointLearningKit > nul
echo List of MLC and SLK DLLs in the Global Assembly Cache:
gacutil /nologo /l Microsoft.LearningComponents.MRCI | findstr /c:Version=
gacutil /nologo /l Microsoft.LearningComponents.Compression | findstr /c:Version=
gacutil /nologo /l Microsoft.LearningComponents | findstr /c:Version=
gacutil /nologo /l Microsoft.LearningComponents.SharePoint | findstr /c:Version=
gacutil /nologo /l Microsoft.LearningComponents.Storage | findstr /c:Version=
gacutil /nologo /l Microsoft.SharePointLearningKit | findstr /c:Version=
