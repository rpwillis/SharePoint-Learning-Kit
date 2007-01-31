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
mkdir LanguagePacks
cd LanguagePacks
if exist %1 del /f/s/q %1
mkdir %1
cd %1
xcopy ..\..\LangPack\*.* /y/r
cd ..\..\..\TranslatedXMLs
goto :end
:end