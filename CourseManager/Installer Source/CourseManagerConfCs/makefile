#  Copyright (c) Microsoft Corporation. All rights reserved. 

# CourseManagerConfCs.sln makefile

all: deb rel

deb:
	devenv CourseManagerConfCs.sln /rebuild Debug 

rel:
	devenv CourseManagerConfCs.sln /rebuild Release 
ide:
	devenv CourseManagerConfCs.sln

clean: 
	rem -- devenv CourseManagerConfCs.sln /clean Debug
	rem -- devenv CourseManagerConfCs.sln /clean Release
 	-rmdir /s /q bin 2> nul
	-rmdir /s /q obj 2> nul

cleanall: clean 
	-attrib -h -r CourseManagerConfCs.suo > nul
	-del CourseManagerConfCs.suo 2> nul
	-del CourseManagerConfCs.ncb 2> nul

