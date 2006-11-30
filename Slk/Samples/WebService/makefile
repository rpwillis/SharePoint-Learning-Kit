#  Copyright (c) Microsoft Corporation. All rights reserved. 

# SampleWebService makefile

all: deb rel

# "nmake deb" just does a test build to make sure the .asmx files compile
deb:
	if exist \SharePointDlls\Microsoft.SharePoint.dll copy /y \SharePointDlls\Microsoft.SharePoint.dll TestBuild\bin
	for %f in (*.asmx) do cscript /nologo ..\..\..\Tools\CopyAndSetVersion.js %f TestBuild\%f
	cd TestBuild
	devenv TestBuild.sln /build Debug
	cd $(MAKEDIR)

rel:
	rem -- nothing to do

ide:
	rem -- nothing to do

clean cleanall:
	for %f in (*.asmx) do del TestBuild\%f 2> nul
	-attrib -h -r TestBuild\TestBuild.suo > nul
	-del TestBuild\TestBuild.suo 2> nul
	-del TestBuild\Bin\*.dll 2> nul
	-del TestBuild\Bin\*.exe 2> nul
	-del TestBuild\Bin\*.pdb 2> nul
	-del TestBuild\Web.Config 2> nul
