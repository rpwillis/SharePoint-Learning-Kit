rem Copyright (c) Microsoft Corporation. All rights reserved. 

@echo off

IF NOT "%1"=="TARGET_ARCH" goto END

Tools\FilterWithIni Tools\Assemble.ini nmake %1=%2 %3 %4 %5 %6 %7 %8 %9 

:END

echo --
echo "Mention TARGET_ARCH=<input architecture> as the first parameter"
echo "Example: fmake TARGET_ARCH=x64 rel"
echo --