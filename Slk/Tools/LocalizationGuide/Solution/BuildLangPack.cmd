@Echo off
if not "%1"=="" (
	goto :build
	)
Echo Usage: BuildLangPack.cmd LCID
goto :end
:build
del /f/s/q LangPack
mkdir LangPack
echo %1 > LangPack\lcid.txt
makecab /f cab.ddf
xcopy *solution.cmd LangPack /y/r
xcopy *solutions.cmd LangPack /y/r
For /F "Tokens=*" %%I in ('more culture.txt') Do Set StrCulture=%%I
mkdir LanguagePacks
cd LanguagePacks
if exist %1 del /f/s/q %1
mkdir %StrCulture%
cd %StrCulture%
xcopy ..\..\LangPack\*.* /y/r
rmdir /S /Q ..\..\LangPack 2> nul
cd ..\..\..\TranslatedXMLs
goto :end
:end