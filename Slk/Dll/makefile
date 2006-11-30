#  Copyright (c) Microsoft Corporation. All rights reserved. 

# Microsoft.SharePointLearningKit.dll makefile

all: deb rel

deb:
	devenv SlkDll.sln /build Debug

rel:
	devenv SlkDll.sln /build Release 

ide:
	devenv SlkDll.sln

skipverification:
	sn -Vr bin\debug\Microsoft.SharePointLearningKit.dll

clean: 
	rem -- devenv SlkDll.sln /clean Debug
	rem -- devenv SlkDll.sln /clean Release
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del SlkDll.csproj.user 2> nul
	-attrib -h -r SlkDll.suo > nul
	-del SlkDll.suo 2> nul
	-del SlkDll.ncb 2> nul
	-rmdir /s /q ApiRef 2> nul

