@Echo off
if not "%1"=="" (
	goto :build
	)
Echo Usage: BuildLangPack.cmd LCID
goto :end

:build
del /f/s/q LangPack
mkdir LangPack
copy lcid.txt LangPack\lcid.txt
For /F "Tokens=*" %%I in ('more culture.txt') Do Set StrCulture=%%I

REM Set up culture specific resource file
..\Localize r manifest.xml manifest.%StrCulture%.xml %StrCulture%

makecab /f cab.ddf /D Culture=%StrCulture%
del manifest.%StrCulture%.xml 2> nul
xcopy scripts\* LangPack /y/r
Set langPacksDir=..\..\..\..\drop\LanguagePacks
if not exist %langPacksDir% mkdir %langPacksDir%
if exist %langPacksDir%\%1 del /f/s/q %langPacksDir%\%1
if not exist %langPacksDir%\%StrCulture% mkdir %langPacksDir%\%StrCulture%
xcopy LangPack\*.* %langPacksDir%\%StrCulture% /y/r
rmdir /S /Q LangPack 2> nul
cd ..\TranslatedXMLs
goto :end
:end
