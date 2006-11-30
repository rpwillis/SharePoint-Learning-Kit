#  Copyright (c) Microsoft Corporation. All rights reserved. 

# Microsoft.SharePointLearningKit.dll makefile

all: deb rel

deb:
	devenv TestDomainGroups.sln /build Debug

rel:
	devenv TestDomainGroups.sln /build Release 

ide:
	devenv TestDomainGroups.sln

clean: 
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del TestDomainGroups.csproj.user 2> nul
	-attrib -h -r TestDomainGroups.suo > nul
	-del TestDomainGroups.suo 2> nul
	-del TestDomainGroups.ncb 2> nul
	-del AppResources.Designer.cs 2> nul
	-del AppResources.resx 2> nul

