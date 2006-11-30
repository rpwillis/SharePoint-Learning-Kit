@echo off
rem  Copyright (c) Microsoft Corporation. All rights reserved. 
rem -- installs MLC and SLK DLLs into the Global Assembly Cache
gacutil /nologo /i ..\SLK.SDK\Debug\Microsoft.LearningComponents.Compression.dll
gacutil /nologo /i Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.dll
gacutil /nologo /i Src\SharePoint\bin\Debug\Microsoft.LearningComponents.SharePoint.dll
gacutil /nologo /i Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.dll
gacutil /nologo /i Slk\Dll\bin\Debug\Microsoft.SharePointLearningKit.dll
echo List of MLC and SLK DLLs in the Global Assembly Cache:
gacutil /nologo /l Microsoft.LearningComponents.Compression | findstr /c:Version=
gacutil /nologo /l Microsoft.LearningComponents | findstr /c:Version=
gacutil /nologo /l Microsoft.LearningComponents.SharePoint | findstr /c:Version=
gacutil /nologo /l Microsoft.LearningComponents.Storage | findstr /c:Version=
gacutil /nologo /l Microsoft.SharePointLearningKit | findstr /c:Version=
echo Restarting IIS...
if not "%1"=="noiisreset" iisreset
