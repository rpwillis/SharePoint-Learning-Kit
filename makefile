#  Copyright (c) Microsoft Corporation. All rights reserved. 

# makefile
#
# Implements the following targets:
#   -- all: same as "nmake deb rel slk samplesdeb samplesrel loc"
#   -- deb: builds Debug configurations of all sources
#   -- rel: builds Release configurations of all sources
#   -- samplesdeb: builds Debug configuration of samples
#   -- samplesrel: builds Release configuration of some samples
#   -- slk: builds SLK Application components
#   -- slkdeb: builds Debug configuration of SLK Application components
#   -- loc: builds localized versions of SLK
#   -- runtests: runs unit tests
#   -- drop: creates Drop directory, which contains files to be copied to the
#      network drop location by the build machine
#   -- clean: deletes all files which this makefile can regenerate
#   -- cleanall: like "clean", but also deletes all files that IDE can
#      regenerate; after "nmake cleanall" there should be no read-write files
#      except checked-out source files

# "all" target (default target): builds everything
SRCDIRS = \
	Src\Compression.CMD Src\LearningComponents.CMD Src\Storage.CMD Src\SharePoint.CMD

# CodeDoc is the location of the CodeDoc (see www.dwell.net)
CODEDOC=..\SLK.Internal\Tools\CodeDoc\CodeDoc.exe

# INSTALL_DIR and SDK_DIR are used in the drop targets
!IF "$(TARGET_ARCH)" == "x64"
INSTALL_DIR=Drop\Drop\Install64
SDK_DIR=Drop\Drop\SDK64
RELEASEPDB=Drop\ReleasePdb64
!ELSE
INSTALL_DIR=Drop\Drop\Install
SDK_DIR=Drop\Drop\SDK
RELEASEPDB=Drop\ReleasePdb
!ENDIF

# API_TITLE is the title used in API documentation
API_TITLE = "Learning Components"

# API_DLL and API_XML list all .dll and .xml files used to generate API
# reference documentation, respectively; include a "!" character after each
# file name
API_DLL = !\
	Src\LearningComponents\bin\Release\Microsoft.LearningComponents.Compression.dll !\
	Src\LearningComponents\bin\Release\Microsoft.LearningComponents.dll !\
	Src\Storage\bin\Release\Microsoft.LearningComponents.Storage.dll !\
	Src\SharePoint\bin\Release\Microsoft.LearningComponents.SharePoint.dll !\
	Slk\Dll\bin\Release\Microsoft.SharePointLearningKit.dll !\

# (the line above this line must be blank!)
API_XML = !\
	Src\LearningComponents\bin\Release\Microsoft.LearningComponents.Compression.xml !\
	Src\LearningComponents\bin\Release\Microsoft.LearningComponents.xml !\
	Src\Storage\bin\Release\Microsoft.LearningComponents.Storage.xml !\
	Src\SharePoint\bin\Release\Microsoft.LearningComponents.SharePoint.xml !\
	Slk\Dll\bin\Release\Microsoft.SharePointLearningKit.xml !\

# (the line above this line must be blank!)
API_IMG = !\
	Src\LearningComponents\Images\* !\
	Src\Storage\Images\* !\
	Src\SharePoint\Images\* !\

# (the line above this line must be blank!)

# API_DLL_NEWLINES is API_DLL with newlines in place of "!"; similar for
# API_XML_NEWLINES and API_IMG_NEWLINES
API_DLL_NEWLINES = $(API_DLL:!=^
)
API_XML_NEWLINES = $(API_XML:!=^
)
API_IMG_NEWLINES = $(API_IMG:!=^
)

# API_DLL_SPACES is API_DLL with spaces in place of "!"; similar for
# API_XML_SPACES and API_IMG_SPACES
API_DLL_SPACES = $(API_DLL:!= )
API_XML_SPACES = $(API_XML:!= )
API_IMG_SPACES = $(API_IMG:!= )

# "all" target (default target): builds everything
all: deb rel slk samplesdeb samplesrel loc

# "samplesdeb" target: builds Debug samples
samplesdeb: deb
	-mkdir Debug 2> nul
	copy /y Src\Compression\bin\Debug\Microsoft.LearningComponents.Compression.dll Debug
	copy /y Src\Compression\bin\Debug\Microsoft.LearningComponents.Compression.pdb Debug
	copy /y Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.dll Debug
	copy /y Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.pdb Debug
	copy /y Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.dll Debug
	copy /y Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.pdb Debug
	copy /y Src\SharePoint\bin\Debug\Microsoft.LearningComponents.SharePoint.dll Debug
	copy /y Src\SharePoint\bin\Debug\Microsoft.LearningComponents.SharePoint.pdb Debug
	copy /y Slk\Dll\bin\Debug\Microsoft.SharePointLearningKit.dll Debug
	copy /y Slk\Dll\bin\Debug\Microsoft.SharePointLearningKit.pdb Debug
	cd Samples\BasicWebPlayer
	$(MAKE) deb TARGET_ARCH=$(TARGET_ARCH)
	cd $(MAKEDIR)
	cd Samples\ValidatePackage
	$(MAKE) deb
	cd $(MAKEDIR)

# "samplesrel" target: builds Release configuration of some samples
samplesrel:
	cd Samples\ValidatePackage
	$(MAKE) rel
	cd $(MAKEDIR)

# "slk" target: builds SLK Application components
slk: nul
	cd Slk
	$(MAKE)
	cd $(MAKEDIR)

# "slkdeb" target: builds Debug SLK Application components
slkdeb: nul
	cd Slk
	$(MAKE) deb
	cd $(MAKEDIR)

loc: nul
	cd Slk\Tools\LocalizationGuide
	$(MAKE)
	cd $(MAKEDIR)

# "deb" target: builds Debug configurations of specified sources
deb: $(SRCDIRS:CMD=deb) 
$(SRCDIRS:CMD=deb):
    @echo.
    @echo -----------------------------------------------------------------
    @echo Building Debug configuration of $*
    @echo -----------------------------------------------------------------
    cd $*
    $(MAKE) /nologo deb TARGET_ARCH=$(TARGET_ARCH)
    cd $(MAKEDIR)

# "rel" target: builds Release configurations of specified sources
rel: $(SRCDIRS:CMD=rel) 
$(SRCDIRS:CMD=rel):
    @echo.
    @echo -----------------------------------------------------------------
    @echo Building Release configuration of $*
    @echo -----------------------------------------------------------------
    cd $*
    $(MAKE) /nologo rel TARGET_ARCH=$(TARGET_ARCH)
    cd $(MAKEDIR)

# "runtests" target: builds Release configurations of specified sources
runtests: $(SRCDIRS:CMD=runtests) 
$(SRCDIRS:CMD=runtests):
    @echo.
    @echo -----------------------------------------------------------------
    @echo Running tests for $*
    @echo -----------------------------------------------------------------
    cd $*
    $(MAKE) /nologo runtests
    cd $(MAKEDIR)

# drop: create Drop directory containing files to go to network drop location
drop: nul
    @echo.
    @echo -----------------------------------------------------------------
    @echo Creating Drop Directory
    @echo -----------------------------------------------------------------
        if not exist Drop mkdir Drop 2> nul
	if not exist Drop\Drop mkdir Drop\Drop 2> nul

	rem -- Create the Install directory...
	-rmdir /s /q $(INSTALL_DIR) 2> nul
	mkdir $(INSTALL_DIR)
	mkdir $(INSTALL_DIR)\Release
	copy Slk\Solution\*.cmd $(INSTALL_DIR)
	copy Slk\Solution\Release\SharePointLearningKit.wsp $(INSTALL_DIR)\Release
	copy Slk\slkadm\bin\Release\slkadm.exe $(INSTALL_DIR)
	copy Slk\SlkSchema.xml $(INSTALL_DIR)
	copy Slk\SlkSchema.sql $(INSTALL_DIR)
	copy Slk\SlkSettings.xml $(INSTALL_DIR)


	rem -- Create the SDK directory...
	-rmdir /s /q $(SDK_DIR) 2> nul
	mkdir $(SDK_DIR)
	rem -- copy Documentation.htm $(SDK_DIR)
	rem -- mkdir $(SDK_DIR)\Pages
	rem -- copy ApiRef\* $(SDK_DIR)\Pages
	xcopy /I Slk\Solution\DebugFiles $(SDK_DIR)\Debug
	xcopy /I Slk\Solution\ReleaseFiles $(SDK_DIR)\Release
	mkdir $(SDK_DIR)\Samples
	xcopy /I Samples\BasicWebPlayer $(SDK_DIR)\Samples\BasicWebPlayer
	xcopy /I Samples\BasicWebPlayer\App_Code $(SDK_DIR)\Samples\BasicWebPlayer\App_Code
	xcopy /I Samples\BasicWebPlayer\App_Code\Frameset $(SDK_DIR)\Samples\BasicWebPlayer\App_Code\Frameset
	xcopy /I Samples\BasicWebPlayer\App_Data $(SDK_DIR)\Samples\BasicWebPlayer\App_Data
	xcopy /I Samples\BasicWebPlayer\App_GlobalResources $(SDK_DIR)\Samples\BasicWebPlayer\App_GlobalResources
	xcopy /I Samples\BasicWebPlayer\Frameset $(SDK_DIR)\Samples\BasicWebPlayer\Frameset
	xcopy /I Samples\BasicWebPlayer\Frameset\Include $(SDK_DIR)\Samples\BasicWebPlayer\Frameset\Include
	xcopy /I Samples\BasicWebPlayer\Frameset\Images $(SDK_DIR)\Samples\BasicWebPlayer\Frameset\Images
	rem -- xcopy /I Samples\BasicWebPlayer\Frameset\Include\UnitTest $(SDK_DIR)\Samples\BasicWebPlayer\Frameset\Include\UnitTest
	xcopy /I Samples\BasicWebPlayer\Frameset\Theme $(SDK_DIR)\Samples\BasicWebPlayer\Frameset\Theme
	rem -- xcopy /I Samples\BasicWebPlayer\Frameset\UnitTest $(SDK_DIR)\Samples\BasicWebPlayer\Frameset\UnitTest
	xcopy /I Samples\ValidatePackage $(SDK_DIR)\Samples\ValidatePackage
	xcopy /I Samples\ValidatePackage\Properties $(SDK_DIR)\Samples\ValidatePackage\Properties
	mkdir $(SDK_DIR)\Samples\SLK
	xcopy /I Slk\Samples\AddInstructors $(SDK_DIR)\Samples\SLK\AddInstructors
	xcopy /I Slk\Samples\AddToUserWebLists $(SDK_DIR)\Samples\SLK\AddToUserWebLists
	xcopy /I Slk\Samples\CreateAssignments $(SDK_DIR)\Samples\SLK\CreateAssignments
	xcopy /I Slk\Samples\ProvisionFromExcel $(SDK_DIR)\Samples\SLK\ProvisionFromExcel
	xcopy /I Slk\Samples\SimulateClass $(SDK_DIR)\Samples\SLK\SimulateClass
	xcopy /I Slk\Samples\SimulateJobTraining $(SDK_DIR)\Samples\SLK\SimulateJobTraining
	mkdir $(SDK_DIR)\Samples\SLK\ReportPages
	copy Slk\Samples\ReportPages\ReadMe.txt $(SDK_DIR)\Samples\SLK\ReportPages
	cd Slk\Samples\ReportPages
	xcopy /Y *.aspx ..\..\..\$(SDK_DIR)\Samples\SLK\ReportPages\
	cd $(MAKEDIR)
	mkdir $(SDK_DIR)\Samples\SLK\WebService
	copy Slk\Samples\WebService\HelloWorld.zip $(SDK_DIR)\Samples\SLK\WebService
	copy Slk\Samples\WebService\ReadMe.txt $(SDK_DIR)\Samples\SLK\WebService
	cd Slk\Samples\WebService
	xcopy /Y *.asmx ..\..\..\$(SDK_DIR)\Samples\SLK\WebService\
	cd $(MAKEDIR)

	mkdir $(SDK_DIR)\SLK
	copy slk\SlkSchema.xml  $(SDK_DIR)\SLK
	copy slk\Dll\SlkSchema.cs  $(SDK_DIR)\SLK
	copy slk\SlkSchema.sql  $(SDK_DIR)\SLK
	copy slk\SlkSettings.xsd  $(SDK_DIR)\SLK
	copy slk\SlkSettings.xsx  $(SDK_DIR)\SLK
	copy Slk\SlkSettings.xml $(SDK_DIR)\SLK

	rem -- Create the LanguagePacks directory...
	-rmdir /s /q Drop\Drop\LanguagePacks 2> nul
	mkdir Drop\Drop\LanguagePacks
	xcopy Slk\Tools\LocalizationGuide\Solution\LanguagePacks\* Drop\Drop\LanguagePacks\ /E

#rem -- Create the ValidatePackage directory...
#mkdir Drop\Drop\ValidatePackage
#copy Samples\ValidatePackage\bin\Release\ValidatePackage.exe Drop\Drop\ValidatePackage
#copy Src\LearningComponents\bin\Release\Microsoft.LearningComponents.dll Drop\Drop\ValidatePackage
#copy Src\Compression\Compression\bin\Release\Microsoft.LearningComponents.Compression.dll Drop\Drop\ValidatePackage

	rem -- Copy files from Doc directory...
	xcopy /I /Y Doc\Root Drop\Drop
	xcopy /I /S /Y Doc\Root $(INSTALL_DIR)
	xcopy /I /S /Y Doc\Root $(SDK_DIR)
	xcopy /I /S /Y Doc\Root Drop\Drop\ValidatePackage
	xcopy /I /S /Y Doc\Root Drop\Drop\LanguagePacks
	xcopy /I /S /Y Doc\Install $(INSTALL_DIR)
	xcopy /I /S /Y Doc\LanguagePacks Drop\Drop\LanguagePacks

	rem -- Create Solitaire.zip files...
	-if exist ..\SLK.Internal mkdir $(INSTALL_DIR)\Samples
	if exist ..\SLK.Internal ..\SLK.Internal\Tools\DZip.exe Samples\Solitaire $(INSTALL_DIR)\Samples\Solitaire.zip
	-if exist ..\SLK.Internal mkdir $(SDK_DIR)\Samples
	if exist ..\SLK.Internal ..\SLK.Internal\Tools\DZip.exe Samples\Solitaire $(SDK_DIR)\Samples\Solitaire.zip

	rem -- Copy other root-level files...
	copy Src\Schema\InitSchema.sql Drop

	rem -- Copy release PDBs...
	mkdir $(RELEASEPDB)
	copy Src\Compression\bin\Release\Microsoft.LearningComponents.Compression.pdb $(RELEASEPDB)
	copy Src\LearningComponents\bin\Release\Microsoft.LearningComponents.pdb $(RELEASEPDB)
	copy Src\SharePoint\bin\Release\Microsoft.LearningComponents.SharePoint.pdb $(RELEASEPDB)
	copy Src\Storage\bin\Release\Microsoft.LearningComponents.Storage.pdb $(RELEASEPDB)
	copy Slk\Dll\bin\Release\Microsoft.SharePointLearningKit.pdb $(RELEASEPDB)

	rem -- Create drop .zip files -- MUST BE DONE AFTER DROP\DROP IS COMPLETE...
	if exist ..\SLK.Internal cscript /nologo ..\SLK.Internal\Tools\MakeDropZipsBatchFile.js > MakeDropZips.bat
	if exist ..\SLK.Internal MakeDropZips.bat
	rem -- NO MORE DROP-RELATED COMMANDS AFTER THIS POINT

# "clean" target: deletes all files which this makefile can regenerate
clean: $(SRCDIRS:CMD=clean) 
$(SRCDIRS:CMD=clean): cleancommon
    @echo.
    @echo -----------------------------------------------------------------
    @echo Cleaning $*
    @echo -----------------------------------------------------------------
    cd $*
    $(MAKE) /nologo clean TARGET_ARCH=$(TARGET_ARCH)
    cd $(MAKEDIR)

# "cleanall" target: like "clean", but also cleans IDE status files etc.
cleanall: $(SRCDIRS:CMD=cleanall) 
$(SRCDIRS:CMD=cleanall): cleancommon
    @echo.
    @echo -----------------------------------------------------------------
    @echo Cleaning $*
    @echo -----------------------------------------------------------------
    cd $*
    $(MAKE) /nologo cleanall TARGET_ARCH=$(TARGET_ARCH)
    cd $(MAKEDIR)

# "cleancommon" target: common to "clean" and "cleanall"
cleancommon: clean_slk clean_latestsdk clean_samples

# "clean_samples" target: Clean the samples directory
clean_samples:
	cd Samples/BasicWebPlayer
	$(MAKE) cleanall
	cd $(MAKEDIR)
	cd Samples/ValidatePackage
	$(MAKE) cleanall
	cd $(MAKEDIR)

# "clean_slk" target: clean the Drop directory
clean_slk:
	cd Slk
	$(MAKE) cleanall
	cd $(MAKEDIR)

# "clean_drop" target: clean the Drop directory
clean_drop:
	-rmdir /s /q Drop 2> nul
	-del MakeDropZips.bat 2> nul

# "clean_latestsdk" target: clean files created by GetLatestSdkDlls.js
clean_latestsdk:
	-rmdir /s /q Debug 2> nul
	-rmdir /s /q Release 2> nul
	-del InitSchema.sql 2> nul

