#  Copyright (c) Microsoft Corporation. All rights reserved. 

# CodePlex.sln makefile

all: deb rel

deb:
	devenv CodePlex.sln /rebuild Debug 

rel:
	devenv CodePlex.sln /rebuild Release 
ide:
	devenv CodePlex.sln

clean: 
	rem -- devenv CodePlex.sln /clean Debug
	rem -- devenv CodePlex.sln /clean Release
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-attrib -h -r CodePlex.suo > nul
	-del CodePlex.suo 2> nul
	-del CodePlex.ncb 2> nul

