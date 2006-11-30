#  Copyright (c) Microsoft Corporation. All rights reserved. 

# Microsoft.LearningComponents makefile

all: deb rel

deb:
	devenv SharePoint.sln /build Debug

rel:
	devenv SharePoint.sln /build Release 

ide:
	devenv SharePoint.sln

skipverification:
	sn -Vr bin\debug\Microsoft.LearningComponents.SharePoint.dll

clean: 
	devenv SharePoint.sln /clean Debug
	devenv SharePoint.sln /clean Release
	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul
	-rmdir /s /q UnitTests\bin 2> nul
	-rmdir /s /q UnitTests\obj 2> nul
	-rmdir /s /q ApiRef 2> nul
	-rmdir /s /q TestResults 2> nul
	-del ApiRefLog.tmp 2> nul

cleanall: clean 
	-del SharePoint.csproj.user 2> nul
	-attrib -h -r SharePoint.suo > nul
	-del SharePoint.suo 2> nul
	-del SharePoint.ncb 2> nul
	-del UnitTests\UnitTests.csproj.user 2> nul
	-rd /q /s TestResults

