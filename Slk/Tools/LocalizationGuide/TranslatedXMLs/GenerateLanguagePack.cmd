rem  Copyright (c) Microsoft Corporation. All rights reserved. 
rem Usage: GenerateLanguagePack.cmd <LCID> <debug/release>

cd ..
mkdir workingFolder
cd workingFolder
..\localize g ..\TranslatedXMLs\%1 || goto :error

copy *.resources.dll ..\solution
copy ..\TranslatedXMLs\%1\SlkSettings.xml.dat ..\solution
copy ..\TranslatedXMLs\%1\culture.txt ..\solution
copy ..\TranslatedXMLs\%1\slk.resx ..\solution
cd ..\solution
call buildLangPack.cmd %1 %2
del ..\solution\*.resources.dll 2> nul
del ..\solution\SlkSettings.xml.dat 2> nul
del ..\solution\culture.txt 2> nul
del ..\solution\slk.resx 2> nul

del /Q ..\workingFolder  2> nul

goto :EOF

:error
echo FAILED TO BUILD %1
exit /b 1
