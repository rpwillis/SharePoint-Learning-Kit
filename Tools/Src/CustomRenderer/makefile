#  Copyright (c) Microsoft Corporation. All rights reserved. 

# CustomRenderer makefile

all: deb rel

deb:
    devenv CustomRenderer.sln /build Debug

rel:
    devenv CustomRenderer.sln /build Release

ide:
    start CustomRenderer.sln

doc:
	..\bin\Debug\CodeDoc.exe CodeDoc.xml

clean:
	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul
	-rmdir /s /q Drop 2> nul

cleanall: clean
	-del CustomRenderer.csproj.user 2> nul
	-attrib -h -r CustomRenderer.suo > nul
	-del CustomRenderer.suo 2> nul

