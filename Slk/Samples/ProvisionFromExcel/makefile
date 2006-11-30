#  Copyright (c) Microsoft Corporation. All rights reserved. 

# ProvisionFromExcel.exe makefile

all: deb rel

deb:
	devenv ProvisionFromExcel.sln /build Debug

rel:
	devenv ProvisionFromExcel.sln /build Release 

ide:
	devenv ProvisionFromExcel.sln

clean: 
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del ProvisionFromExcel.csproj.user 2> nul
	-attrib -h -r ProvisionFromExcel.suo > nul
	-del ProvisionFromExcel.suo 2> nul
	-del ProvisionFromExcel.ncb 2> nul
	-del ProvisionFromExcel.exe 2> nul

