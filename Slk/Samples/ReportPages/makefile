#  Copyright (c) Microsoft Corporation. All rights reserved. 

# ReportPages makefile

all: deb rel

# "nmake deb" just does a test build to make sure the .aspx files compile
deb:
	for %f in (*.aspx) do cscript /nologo ..\..\..\Tools\CopyAndSetVersion.js %f TestBuild\%f
	cd TestBuild
!IF "$(TARGET_ARCH)" == "x64"
	@echo "Skipping build on 64-bit architectures"
!ELSE
	devenv TestBuild.sln /build Debug
!ENDIF
	cd $(MAKEDIR)


rel:
	rem -- nothing to do

ide:
	rem -- nothing to do

clean cleanall:
	for %f in (*.aspx) do del TestBuild\%f 2> nul
	-attrib -h -r TestBuild\TestBuild.suo > nul
	-del TestBuild\TestBuild.suo 2> nul
	-del TestBuild\Bin\*.dll 2> nul
	-del TestBuild\Bin\*.exe 2> nul
	-del TestBuild\Bin\*.pdb 2> nul
	-del TestBuild\Web.Config 2> nul
