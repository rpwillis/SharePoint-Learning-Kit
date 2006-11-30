#  Copyright (c) Microsoft Corporation. All rights reserved. 

# slkadm.exe makefile

all: deb rel

deb:
	devenv slkadm.sln /build Debug

rel:
	devenv slkadm.sln /build Release 

ide:
	devenv slkadm.sln

clean: 
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del slkadm.csproj.user 2> nul
	-attrib -h -r slkadm.suo > nul
	-del slkadm.suo 2> nul
	-del slkadm.ncb 2> nul

