#  Copyright (c) Microsoft Corporation. All rights reserved. 

# Microsoft.LearningComponents.Compression makefile

all: deb rel

deb:
	devenv Compression.sln /build Debug /Project Microsoft.LearningComponents.MRCI /ProjectConfig "Debug|Win32"
	devenv Compression.sln /build Debug /Project Compression

rel:
	devenv Compression.sln /build Release /Project Microsoft.LearningComponents.MRCI /ProjectConfig "Release|Win32"
	devenv Compression.sln /build Release /Project Compression

ide:
	devenv Compression.sln

skipverification:
	sn -Vr bin\debug\Microsoft.LearningComponents.Compression.dll
	sn -Vr bin\debug\Microsoft.LearningComponents.MRCI.dll
	sn -Vr CompressionUnitTest\bin\Debug\CompressionUnitTest.dll

runtests:
	devenv Compression.sln /build Debug /Project CompressionUnitTest
	MSTest.exe /testcontainer:CompressionUnitTest\bin\Debug\CompressionUnitTest.dll

clean: 
	devenv Compression.sln /clean Debug
	devenv Compression.sln /clean Release
	-rmdir /s /q Compression\bin 2> nul
	-rmdir /s /q Compression\obj 2> nul
	-rmdir /s /q MRCI\bin 2> nul
	-rmdir /s /q MRCI\obj 2> nul
	-rmdir /s /q TestResults 2> nul
	-rmdir /s /q CompressionUnitTest\UnitTestData\UnbundleFolder 2> nul
	-rmdir /s /q CompressionUnitTest\bin 2> nul
	-rmdir /s /q CompressionUnitTest\obj 2> nul

cleanall: clean 
	-del Compression.csproj.user 2> nul
	-attrib -h -r Compression.suo > nul
	-del Compression.suo 2> nul
	-del CompressionUnitTest\CompressionUnitTest.csproj.user 2> nul
	-rd /q /s TestResults

