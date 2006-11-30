#  Copyright (c) Microsoft Corporation. All rights reserved. 

# AddToUserWebLists.exe makefile

all: deb rel

deb:
	devenv AddToUserWebLists.sln /build Debug

rel:
	devenv AddToUserWebLists.sln /build Release 

ide:
	devenv AddToUserWebLists.sln

clean: 
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del AddToUserWebLists.csproj.user 2> nul
	-attrib -h -r AddToUserWebLists.suo > nul
	-del AddToUserWebLists.suo 2> nul
	-del AddToUserWebLists.ncb 2> nul
	-del AddToUserWebLists.exe 2> nul

