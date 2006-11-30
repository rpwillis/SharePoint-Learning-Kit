#  Copyright (c) Microsoft Corporation. All rights reserved. 

# BasicWebPlayer makefile

all: init deb rel

init:
	cmd /c Init.bat

deb: init
	devenv BasicWebPlayer.sln /build Debug

ide:
	devenv BasicWebPlayer.sln

clean: 
	-del Bin\*.dll 2> nul
	-del Bin\*.pdb 2> nul
	-del Bin\_ReadMe.txt 2> nul

cleanall: clean 
	-del BasicWebPlayer.csproj.user 2> nul
	-attrib -h -r BasicWebPlayer.suo > nul
	-del BasicWebPlayer.suo 2> nul
	-del Schema.sql 2> nul
	 del App_Code\Schema.cs 2> nul
 	-rmdir /s /q Bin 2> nul

