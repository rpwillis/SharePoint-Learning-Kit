#  Copyright (c) Microsoft Corporation. All rights reserved. 

# WssSolution makefile

all: rel

deb:
	-rmdir /s /q Build 2> nul
	mkdir Build
	copy ..\..\Src\Compression\MRCI\bin\Debug\Microsoft.LearningComponents.MRCI.dll Build
	copy ..\..\Src\Compression\MRCI\bin\Debug\Microsoft.LearningComponents.MRCI.pdb Build
	SignCode.bat Build\Microsoft.LearningComponents.MRCI.dll
	copy ..\..\Src\Compression\Compression\bin\Debug\Microsoft.LearningComponents.Compression.dll Build
	copy ..\..\Src\Compression\Compression\bin\Debug\Microsoft.LearningComponents.Compression.pdb Build
	SignCode.bat Build\Microsoft.LearningComponents.Compression.dll
	copy ..\..\Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.dll Build
	copy ..\..\Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.pdb Build
	SignCode.bat Build\Microsoft.LearningComponents.dll
	copy ..\..\Src\SharePoint\bin\Debug\Microsoft.LearningComponents.SharePoint.dll Build
	copy ..\..\Src\SharePoint\bin\Debug\Microsoft.LearningComponents.SharePoint.pdb Build
	SignCode.bat Build\Microsoft.LearningComponents.SharePoint.dll
	copy ..\..\Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.dll Build
	copy ..\..\Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.pdb Build
	SignCode.bat Build\Microsoft.LearningComponents.Storage.dll
	copy ..\Dll\bin\Debug\Microsoft.SharePointLearningKit.dll Build
	copy ..\Dll\bin\Debug\Microsoft.SharePointLearningKit.pdb Build
	SignCode.bat Build\Microsoft.SharePointLearningKit.dll
	copy ..\..\Src\SchemaCompiler\bin\Debug\SchemaCompiler.exe Build
	copy ..\..\Src\SchemaCompiler\bin\Debug\SchemaCompiler.pdb Build
	SignCode.bat Build\SchemaCompiler.exe
	-rmdir /s /q Debug 2> nul
	mkdir Debug
	copy Cab.ddf Debug
	cd Debug
	makecab /F Cab.ddf
	copy /y SharePointLearningKit.wsp CabContents.cab
	cd $(MAKEDIR)
	-rmdir /s /q DebugFiles 2> nul
	ren Build DebugFiles

rel:
	-rmdir /s /q Build 2> nul
	mkdir Build
	copy ..\..\Src\Compression\MRCI\bin\Release\Microsoft.LearningComponents.MRCI.dll Build
	SignCode.bat Build\Microsoft.LearningComponents.MRCI.dll
	copy ..\..\Src\Compression\Compression\bin\Release\Microsoft.LearningComponents.Compression.dll Build
	SignCode.bat Build\Microsoft.LearningComponents.Compression.dll
	copy ..\..\Src\LearningComponents\bin\Release\Microsoft.LearningComponents.dll Build
	SignCode.bat Build\Microsoft.LearningComponents.dll
	copy ..\..\Src\SharePoint\bin\Release\Microsoft.LearningComponents.SharePoint.dll Build
	SignCode.bat Build\Microsoft.LearningComponents.SharePoint.dll
	copy ..\..\Src\Storage\bin\Release\Microsoft.LearningComponents.Storage.dll Build
	SignCode.bat Build\Microsoft.LearningComponents.Storage.dll
	copy ..\Dll\bin\Release\Microsoft.SharePointLearningKit.dll Build
	SignCode.bat Build\Microsoft.SharePointLearningKit.dll
	copy ..\..\Src\SchemaCompiler\bin\Release\SchemaCompiler.exe Build
	SignCode.bat Build\SchemaCompiler.exe
	-rmdir /s /q Release 2> nul
	mkdir Release
	copy Cab.ddf Release
	cd Release
	makecab /F Cab.ddf
	copy /y SharePointLearningKit.wsp CabContents.cab
	cd $(MAKEDIR)
	-rmdir /s /q ReleaseFiles 2> nul
	ren Build ReleaseFiles

clean cleanall:
	-rmdir /s /q Build 2> nul
	-rmdir /s /q Debug 2> nul
	-rmdir /s /q DebugFiles 2> nul
	-rmdir /s /q Release 2> nul
	-rmdir /s /q ReleaseFiles 2> nul

