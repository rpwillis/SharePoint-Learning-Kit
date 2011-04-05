#  Copyright (c) Microsoft Corporation. All rights reserved. 

# ValidatePackage.exe makefile

all: deb rel

deb:
	devenv ValidatePackage.sln /build Debug

rel:
	devenv ValidatePackage.sln /build Release 

ide:
	devenv ValidatePackage.sln

clean: 
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del ValidatePackage.csproj.user 2> nul
	-attrib -h -r ValidatePackage.suo > nul
	-del ValidatePackage.suo 2> nul
	-del ValidatePackage.ncb 2> nul

