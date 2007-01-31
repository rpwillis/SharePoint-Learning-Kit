rem  Copyright (c) Microsoft Corporation. All rights reserved. 
rem Usage: GenerateLanguagePack.cmd <LCID> <debug/release>

cd ..
del /f/s/q bin 
mkdir bin
cd bin
copy ..\%2\*.dll 
..\localize g ..\TranslatedXMLs\%1\LearningComponents.xml
..\localize g  ..\TranslatedXMLs\%1\SharePointLearningKit.xml
..\localize g  ..\TranslatedXMLs\%1\SharePoint.xml
..\localize g  ..\TranslatedXMLs\%1\Storage.xml

copy *.resources.dll ..\solution
copy ..\TranslatedXMLs\%1\SlkSettings.xml.dat ..\solution
cd ..\solution
call ..\SignCode.bat Microsoft.LearningComponents.resources.dll
call ..\SignCode.bat Microsoft.LearningComponents.SharePoint.resources.dll
call ..\SignCode.bat Microsoft.LearningComponents.Storage.resources.dll
call ..\SignCode.bat Microsoft.SharePointLearningKit.resources.dll
call buildLangPack.cmd %1


