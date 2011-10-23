#  Copyright (c) Microsoft Corporation. All rights reserved. 

# CreateAssignments.exe makefile

all: deb rel

deb:
	devenv CreateAssignments.sln /build Debug

rel:
	devenv CreateAssignments.sln /build Release 

ide:
	devenv CreateAssignments.sln

clean: 
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del CreateAssignments.csproj.user 2> nul
	-attrib -h -r CreateAssignments.suo > nul
	-del CreateAssignments.suo 2> nul
	-del CreateAssignments.ncb 2> nul
	-del CreateAssignments.exe 2> nul

