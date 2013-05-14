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
Set langPacksDir=..\..\..\..\drop\LanguagePacks
if not exist %langPacksDir% mkdir %langPacksDir%
if exist %langPacksDir%\%1 del /f/s/q %langPacksDir%\%1
if not exist %langPacksDir%\%StrCulture% mkdir %langPacksDir%\%StrCulture%
xcopy LangPack\*.* %langPacksDir%\%StrCulture% /y/r
rmdir /S /Q LangPack 2> nul
cd ..\TranslatedXMLs
goto :end
:end
