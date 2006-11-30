@echo off
REM --  Copyright (c) Microsoft Corporation. All rights reserved. 

if "%1"=="Clone" goto DoClone
if "%1"=="Clean" goto DoClean
echo BAD LINKFILE.BAT COMMAND
goto exit

:DoClone
if exist %3 del /f %3 2>nul
copy /y %2 %3
attrib +r %3
goto exit

:DoClean
if exist %3 del /f %3 2>nul
goto exit

:exit
@echo on
