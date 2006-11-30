#  Copyright (c) Microsoft Corporation. All rights reserved. 

# XmlDocPreprocessor makefile

all: deb rel

deb:
	devenv XmlDocPreprocessor.sln /build Debug

rel:
	devenv XmlDocPreprocessor.sln /build Release 

ide:
	devenv XmlDocPreprocessor.sln

clean: 
	devenv XmlDocPreprocessor.sln /clean Debug
	devenv XmlDocPreprocessor.sln /clean Release
	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del XmlDocPreprocessor.csproj.user 2> nul
	-attrib -h -r XmlDocPreprocessor.suo > nul
	-del XmlDocPreprocessor.suo 2> nul

