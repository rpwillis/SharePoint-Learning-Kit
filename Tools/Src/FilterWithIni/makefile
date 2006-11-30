#  Copyright (c) Microsoft Corporation. All rights reserved. 

# FilterWithIni makefile

all: deb rel

deb:
    devenv FilterWithIni.sln /build Debug

rel:
    devenv FilterWithIni.sln /build Release
	copy bin\Release\FilterWithIni.exe ..\..

ide:
    start FilterWithIni.sln

clean:
	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean
	-del FilterWithIni.csproj.user 2> nul
	-attrib -h -r FilterWithIni.suo > nul
	-del FilterWithIni.suo 2> nul

foo:
	echo This line is successful.
	echo That has an error in it.
	echo This line has an error that is suppressed due to Errors.xls.
	echo The end.

bar:
	echo This simulates a successful build.
