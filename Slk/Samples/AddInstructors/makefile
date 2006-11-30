#  Copyright (c) Microsoft Corporation. All rights reserved. 

# AddInstructorsSample.exe makefile

all: deb rel

deb:
	devenv AddInstructorsSample.sln /build Debug

rel:
	devenv AddInstructorsSample.sln /build Release 

ide:
	devenv AddInstructorsSample.sln

clean: 
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del AddInstructorsSample.csproj.user 2> nul
	-attrib -h -r AddInstructorsSample.suo > nul
	-del AddInstructorsSample.suo 2> nul
	-del AddInstructorsSample.ncb 2> nul

