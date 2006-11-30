#  Copyright (c) Microsoft Corporation. All rights reserved. 

# FixDocIndex makefile

all: deb rel

deb:
    devenv FixDocIndex.sln /build Debug

rel:
    devenv FixDocIndex.sln /build Release
	copy bin\Release\FixDocIndex.exe ..\..

ide:
    start FixDocIndex.sln

clean:
	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean
	-del FixDocIndex.csproj.user 2> nul
	-attrib -h -r FixDocIndex.suo > nul
	-del FixDocIndex.suo 2> nul

