#  Copyright (c) Microsoft Corporation. All rights reserved. 

 # Microsoft.LearningComponents.Storage makefile

all: deb rel

deb:
	devenv SchemaCompiler.sln /build Debug

rel:
	devenv SchemaCompiler.sln /build Release 
	devenv SchemaCompiler.sln /build Debug

ide:
	devenv SchemaCompiler.sln

clean: 
	devenv SchemaCompiler.sln /clean Debug
	devenv SchemaCompiler.sln /clean Release
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul
	-rmdir /s /q Test\bin 2> nul
	-rmdir /s /q Test\obj 2> nul
	-rmdir /s /q TestResults 2> nul
	-rmdir /s /q Test\TestResults 2> nul

cleanall: clean 
	-del SchemaCompiler.csproj.user 2> nul
	-attrib -h -r SchemaCompiler.suo > nul
	-del SchemaCompiler.suo 2> nul
	-del SchemaCompiler.ncb 2> nul
	-del Schema.xsx 2> nul
	-attrib -h -r Test\SchemaCompiler.suo > nul
	-del Test\SchemaCompiler.suo > nul
	-del Test\SchemaCompiler.csproj.user > nul
	-del Test\SchemaCompiler.ncb > nul

