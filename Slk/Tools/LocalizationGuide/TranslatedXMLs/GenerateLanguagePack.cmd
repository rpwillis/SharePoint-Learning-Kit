rem  Copyright (c) Microsoft Corporation. All rights reserved. 
rem Usage: GenerateLanguagePack.cmd <LCID> <debug/release>

cd ..
del /f/s/q bin 
mkdir bin
cd bin
..\localize g ..\TranslatedXMLs\%1

copy *.resources.dll ..\solution
copy ..\TranslatedXMLs\%1\SlkSettings.xml.dat ..\solution
copy ..\TranslatedXMLs\%1\culture.txt ..\solution
cd ..\solution
call buildLangPack.cmd %1 %2
del ..\solution\*.resources.dll 2> nul
del ..\solution\SlkSettings.xml.dat 2> nul
del ..\solution\culture.txt 2> nul
