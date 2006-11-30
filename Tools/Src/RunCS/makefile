#  Copyright (c) Microsoft Corporation. All rights reserved. 

# RunCS makefile

all: deb rel

deb:
	devenv RunCS.sln /build Debug

rel:
	devenv RunCS.sln /build Release 

ide:
	devenv RunCS.sln

clean: 
	devenv RunCS.sln /clean Debug
	devenv RunCS.sln /clean Release
	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del RunCS.csproj.user 2> nul
	-attrib -h -r RunCS.suo > nul
	-del RunCS.suo 2> nul

