#  Copyright (c) Microsoft Corporation. All rights reserved. 

# SimulateClass.exe makefile

all: deb rel

deb:
	devenv SimulateClass.sln /build Debug

rel:
	devenv SimulateClass.sln /build Release 

ide:
	devenv SimulateClass.sln

clean: 
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del SimulateClass.csproj.user 2> nul
	-attrib -h -r SimulateClass.suo > nul
	-del SimulateClass.suo 2> nul
	-del SimulateClass.ncb 2> nul
	-del SimulateClass.exe 2> nul

