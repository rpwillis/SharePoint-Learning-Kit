#  Copyright (c) Microsoft Corporation. All rights reserved. 

 # Microsoft.LearningComponents.Storage makefile

all: deb rel

deb:
	devenv Storage.sln /build Debug

rel:
	devenv Storage.sln /build Release 

ide:
	devenv Storage.sln

skipverification:
	sn -Vr bin\debug\Microsoft.LearningComponents.Storage.dll

runtests:

clean: 
	devenv Storage.sln /clean Debug
	devenv Storage.sln /clean Release
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del Storage.csproj.user 2> nul
	-attrib -h -r Storage.suo > nul
	-del Storage.suo 2> nul
	-del Storage.ncb 2> nul

