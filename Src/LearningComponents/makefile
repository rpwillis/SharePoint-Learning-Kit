#  Copyright (c) Microsoft Corporation. All rights reserved. 

# Microsoft.LearningComponents makefile

all: deb rel

deb:
	devenv LearningComponents.sln /build Debug

rel:
	devenv LearningComponents.sln /build Release 

ide:
	devenv LearningComponents.sln

skipverification:
	sn -Vr bin\debug\Microsoft.LearningComponents.dll
	sn -Vr Test\bin\Debug\LearningComponentsUnitTests.dll

runtests:

clean: 
	devenv LearningComponents.sln /clean Debug
	devenv LearningComponents.sln /clean Release
	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul
	-rmdir /s /q UnitTests\bin 2> nul
	-rmdir /s /q UnitTests\obj 2> nul
	-rmdir /s /q ApiRef 2> nul
	-rmdir /s /q TestResults 2> nul
	-del ApiRefLog.tmp 2> nul

cleanall: clean 
	-del LearningComponents.csproj.user 2> nul
	-attrib -h -r LearningComponents.suo > nul
	-del LearningComponents.suo 2> nul
	-del UnitTests\UnitTests.csproj.user 2> nul
	-rd /q /s TestResults

