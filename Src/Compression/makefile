#  Copyright (c) Microsoft Corporation. All rights reserved. 

# Microsoft.LearningComponents.Compression makefile


# checking the target platform
!IF "$(TARGET_ARCH)" == "x64"
TARGET_BUILD=x64
!ELSE
TARGET_BUILD=Win32
!ENDIF

# all
all: deb rel

deb:
	devenv Compression.sln /rebuild Debug /Project Microsoft.LearningComponents.MRCI /ProjectConfig "Debug|$(TARGET_BUILD)"
	devenv Compression.sln /rebuild Debug /Project Compression

rel:
	devenv Compression.sln /rebuild Release /Project Microsoft.LearningComponents.MRCI /ProjectConfig "Release|$(TARGET_BUILD)"
	devenv Compression.sln /rebuild Release /Project Compression

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

