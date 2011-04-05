#  Copyright (c) Microsoft Corporation. All rights reserved. 

# SimulateJobTraining.exe makefile

all: deb rel

deb:
	devenv SimulateJobTraining.sln /build Debug

rel:
	devenv SimulateJobTraining.sln /build Release 

ide:
	devenv SimulateJobTraining.sln

clean: 
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-del SimulateJobTraining.csproj.user 2> nul
	-attrib -h -r SimulateJobTraining.suo > nul
	-del SimulateJobTraining.suo 2> nul
	-del SimulateJobTraining.ncb 2> nul
	-del SimulateJobTraining.exe 2> nul

