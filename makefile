#  Copyright (c) Microsoft Corporation. All rights reserved. 

# makefile
#
# Implements the following targets:
#   -- all: same as "nmake deb rel slk sdkdoc samplesdeb samplesrel"
#   -- deb: builds Debug configurations of all sources
#   -- rel: builds Release configurations of all sources
#   -- samplesdeb: builds Debug configuration of samples
#   -- samplesrel: builds Release configuration of some samples
#   -- slk: builds SLK Application components
#   -- slkdeb: builds Debug configuration of SLK Application components
#   -- runtests: runs unit tests
#   -- sdkdoc: builds SDK documentation (do "deb" or "rel" first)
#   -- drop: creates Drop directory, which contains files to be copied to the
#      network drop location by the build machine
#   -- clean: deletes all files which this makefile can regenerate
#   -- cleanall: like "clean", but also deletes all files that IDE can
#      regenerate; after "nmake cleanall" there should be no read-write files
#      except checked-out source files

# "all" target (default target): builds everything
SRCDIRS = \
	Src\SchemaCompiler.CMD Src\Schema.CMD Src\LearningComponents.CMD Src\Storage.CMD Src\SharePoint.CMD

# CodeDoc is the location of the CodeDoc (see www.dwell.net)
CODEDOC=..\SLK.Internal\Tools\CodeDoc\CodeDoc.exe

# API_TITLE is the title used in API documentation
API_TITLE = "Learning Components"

# API_DLL and API_XML list all .dll and .xml files used to generate API
# reference documentation, respectively; include a "!" character after each
# file name
API_DLL = !\
	Src\LearningComponents\bin\Release\Microsoft.LearningComponents.dll !\
	Src\Storage\bin\Release\Microsoft.LearningComponents.Storage.dll !\
	Src\SharePoint\bin\Release\Microsoft.LearningComponents.SharePoint.dll !\
	Slk\Dll\bin\Release\Microsoft.SharePointLearningKit.dll !\

# (the line above this line must be blank!)
API_XML = !\
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
all: deb rel slk sdkdoc samplesdeb samplesrel

# "samplesdeb" target: builds Debug samples
samplesdeb: deb
	-mkdir Debug 2> nul
	copy /y ..\SLK.SDK\Debug\Microsoft.LearningComponents.Compression.dll Debug
	copy /y ..\SLK.SDK\Debug\Microsoft.LearningComponents.Compression.pdb Debug
	copy /y Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.dll Debug
	copy /y Src\LearningComponents\bin\Debug\Microsoft.LearningComponents.pdb Debug
	copy /y Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.dll Debug
	copy /y Src\Storage\bin\Debug\Microsoft.LearningComponents.Storage.pdb Debug
	copy /y Src\SchemaCompiler\bin\Debug\SchemaCompiler.exe Debug
	copy /y Src\SchemaCompiler\bin\Debug\SchemaCompiler.pdb Debug
	copy /y Src\SharePoint\bin\Debug\Microsoft.LearningComponents.SharePoint.dll Debug
	copy /y Src\SharePoint\bin\Debug\Microsoft.LearningComponents.SharePoint.pdb Debug
	copy /y Slk\Dll\bin\Debug\Microsoft.SharePointLearningKit.dll Debug
	copy /y Slk\Dll\bin\Debug\Microsoft.SharePointLearningKit.pdb Debug
	cd Samples\BasicWebPlayer
	$(MAKE) deb
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

# "deb" target: builds Debug configurations of specified sources
deb: $(SRCDIRS:CMD=deb) 
$(SRCDIRS:CMD=deb):
    @echo.
    @echo -----------------------------------------------------------------
    @echo Building Debug configuration of $*
    @echo -----------------------------------------------------------------
    cd $*
    $(MAKE) /nologo deb
    cd $(MAKEDIR)

# "rel" target: builds Release configurations of specified sources
rel: $(SRCDIRS:CMD=rel) 
$(SRCDIRS:CMD=rel):
    @echo.
    @echo -----------------------------------------------------------------
    @echo Building Release configuration of $*
    @echo -----------------------------------------------------------------
    cd $*
    $(MAKE) /nologo rel
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

# sdkdoc: builds SDK documentation
sdkdoc: nul
	if exist $(CODEDOC) cscript Tools\MakeSchemaDataStorage2.js
	-if exist $(CODEDOC) $(CODEDOC) CodeDoc.xml
	if exist $(CODEDOC) Tools\FixDocIndex SdkDoc\Pages\Default_Index.htm
	if exist $(CODEDOC) cscript Tools\FixSdkDoc.js

# drop: create Drop directory containing files to go to network drop location
drop: nul
    @echo.
    @echo -----------------------------------------------------------------
    @echo Creating Drop Directory
    @echo -----------------------------------------------------------------
	-rmdir /s /q Drop 2> nul
	mkdir Drop 2> nul
	mkdir Drop\Drop 2> nul

	rem -- Create the Install directory...
	mkdir Drop\Drop\Install
	mkdir Drop\Drop\Install\Release
	copy Slk\Solution\*.cmd Drop\Drop\Install
	copy Slk\Solution\Release\SharePointLearningKit.wsp Drop\Drop\Install\Release
	copy Slk\slkadm\bin\Release\slkadm.exe Drop\Drop\Install
	copy Slk\SlkSchema.xml Drop\Drop\Install
	copy Slk\SlkSchema.sql Drop\Drop\Install
	copy Slk\SlkSettings.xml Drop\Drop\Install

	rem -- Create the SDK directory...
	mkdir Drop\Drop\SDK
	rem -- copy Documentation.htm Drop\Drop\SDK
	rem -- mkdir Drop\Drop\SDK\Pages
	rem -- copy ApiRef\* Drop\Drop\SDK\Pages
	xcopy /Q /I /S SdkDoc Drop\Drop\SDK
	cscript Tools\CopyAndSetVersion.js SdkDoc\Pages\Default.htm Drop\Drop\SDK\Pages\Default.htm
	xcopy /I Slk\Solution\DebugFiles Drop\Drop\SDK\Debug
	xcopy /I Slk\Solution\ReleaseFiles Drop\Drop\SDK\Release
	mkdir Drop\Drop\SDK\Samples
	xcopy /I Samples\BasicWebPlayer Drop\Drop\SDK\Samples\BasicWebPlayer
	xcopy /I Samples\BasicWebPlayer\App_Code Drop\Drop\SDK\Samples\BasicWebPlayer\App_Code
	xcopy /I Samples\BasicWebPlayer\App_Code\Frameset Drop\Drop\SDK\Samples\BasicWebPlayer\App_Code\Frameset
	xcopy /I Samples\BasicWebPlayer\App_Data Drop\Drop\SDK\Samples\BasicWebPlayer\App_Data
	xcopy /I Samples\BasicWebPlayer\App_GlobalResources Drop\Drop\SDK\Samples\BasicWebPlayer\App_GlobalResources
	xcopy /I Samples\BasicWebPlayer\Frameset Drop\Drop\SDK\Samples\BasicWebPlayer\Frameset
	xcopy /I Samples\BasicWebPlayer\Frameset\Include Drop\Drop\SDK\Samples\BasicWebPlayer\Frameset\Include
	rem -- xcopy /I Samples\BasicWebPlayer\Frameset\Include\UnitTest Drop\Drop\SDK\Samples\BasicWebPlayer\Frameset\Include\UnitTest
	xcopy /I Samples\BasicWebPlayer\Frameset\Theme Drop\Drop\SDK\Samples\BasicWebPlayer\Frameset\Theme
	rem -- xcopy /I Samples\BasicWebPlayer\Frameset\UnitTest Drop\Drop\SDK\Samples\BasicWebPlayer\Frameset\UnitTest
	xcopy /I Samples\ValidatePackage Drop\Drop\SDK\Samples\ValidatePackage
	xcopy /I Samples\ValidatePackage\Properties Drop\Drop\SDK\Samples\ValidatePackage\Properties
	mkdir Drop\Drop\SDK\Samples\SLK
	xcopy /I Slk\Samples\AddInstructors Drop\Drop\SDK\Samples\SLK\AddInstructors
	xcopy /I Slk\Samples\AddToUserWebLists Drop\Drop\SDK\Samples\SLK\AddToUserWebLists
	xcopy /I Slk\Samples\CreateAssignments Drop\Drop\SDK\Samples\SLK\CreateAssignments
	xcopy /I Slk\Samples\ProvisionFromExcel Drop\Drop\SDK\Samples\SLK\ProvisionFromExcel
	xcopy /I Slk\Samples\SimulateClass Drop\Drop\SDK\Samples\SLK\SimulateClass
	xcopy /I Slk\Samples\SimulateJobTraining Drop\Drop\SDK\Samples\SLK\SimulateJobTraining
	mkdir Drop\Drop\SDK\Samples\SLK\ReportPages
	copy Slk\Samples\ReportPages\ReadMe.txt Drop\Drop\SDK\Samples\SLK\ReportPages
	cd Slk\Samples\ReportPages
	for %f in (*.aspx) do cscript /nologo ..\..\..\Tools\CopyAndSetVersion.js %f ..\..\..\Drop\Drop\SDK\Samples\SLK\ReportPages\%f
	cd $(MAKEDIR)
	mkdir Drop\Drop\SDK\Samples\SLK\WebService
	copy Slk\Samples\WebService\HelloWorld.zip Drop\Drop\SDK\Samples\SLK\WebService
	copy Slk\Samples\WebService\ReadMe.txt Drop\Drop\SDK\Samples\SLK\WebService
	cd Slk\Samples\WebService
	for %f in (*.asmx) do cscript /nologo ..\..\..\Tools\CopyAndSetVersion.js %f ..\..\..\Drop\Drop\SDK\Samples\SLK\WebService\%f
	cd $(MAKEDIR)

	mkdir Drop\Drop\SDK\SLK
	copy Slk\SlkSdk\* Drop\Drop\SDK\SLK
	copy Slk\SlkSettings.xml Drop\Drop\SDK\SLK
	copy Src\SchemaCompiler\Schema.xsd Drop\Drop\SDK

	rem -- Create the SourceCode directory...
	mkdir Drop\Drop\SourceCode
	mkdir Drop\Drop\SourceCode\Samples
	xcopy /I /S Samples\Solitaire Drop\Drop\SourceCode\Samples\Solitaire
	rem --
	xcopy /I Samples\BasicWebPlayer Drop\Drop\SourceCode\Samples\BasicWebPlayer
	xcopy /I Samples\BasicWebPlayer\App_Code Drop\Drop\SourceCode\Samples\BasicWebPlayer\App_Code
	xcopy /I Samples\BasicWebPlayer\App_Code\Frameset Drop\Drop\SourceCode\Samples\BasicWebPlayer\App_Code\Frameset
	xcopy /I Samples\BasicWebPlayer\App_Data Drop\Drop\SourceCode\Samples\BasicWebPlayer\App_Data
	xcopy /I Samples\BasicWebPlayer\App_GlobalResources Drop\Drop\SourceCode\Samples\BasicWebPlayer\App_GlobalResources
	xcopy /I Samples\BasicWebPlayer\Frameset Drop\Drop\SourceCode\Samples\BasicWebPlayer\Frameset
	xcopy /I Samples\BasicWebPlayer\Frameset\Include Drop\Drop\SourceCode\Samples\BasicWebPlayer\Frameset\Include
	rem -- xcopy /I Samples\BasicWebPlayer\Frameset\Include\UnitTest Drop\Drop\SourceCode\Samples\BasicWebPlayer\Frameset\Include\UnitTest
	xcopy /I Samples\BasicWebPlayer\Frameset\Theme Drop\Drop\SourceCode\Samples\BasicWebPlayer\Frameset\Theme
	rem -- xcopy /I Samples\BasicWebPlayer\Frameset\UnitTest Drop\Drop\SourceCode\Samples\BasicWebPlayer\Frameset\UnitTest
	rem --
	mkdir Drop\Drop\SourceCode\Slk
	xcopy /I Slk\Admin Drop\Drop\SourceCode\Slk\Admin
	xcopy /I Slk\AdminFeature Drop\Drop\SourceCode\Slk\AdminFeature
	xcopy /I Slk\App Drop\Drop\SourceCode\Slk\App
	xcopy /I Slk\App\_layouts Drop\Drop\SourceCode\Slk\App\_layouts
	xcopy /I Slk\App\Frameset Drop\Drop\SourceCode\Slk\App\Frameset
	xcopy /I Slk\App\Frameset\Include Drop\Drop\SourceCode\Slk\App\Frameset\Include
	xcopy /I Slk\App\Frameset\Theme Drop\Drop\SourceCode\Slk\App\Frameset\Theme
	xcopy /I Slk\App\Images Drop\Drop\SourceCode\Slk\App\Images
	xcopy /I Slk\AppFeature Drop\Drop\SourceCode\Slk\AppFeature
	xcopy /I Slk\Dll Drop\Drop\SourceCode\Slk\Dll
	xcopy /I Slk\Dll\AdminWebPages Drop\Drop\SourceCode\Slk\Dll\AdminWebPages
	xcopy /I Slk\Dll\AppWebControls Drop\Drop\SourceCode\Slk\Dll\AppWebControls
	xcopy /I Slk\Dll\AppWebPages Drop\Drop\SourceCode\Slk\Dll\AppWebPages
	xcopy /I Slk\Dll\AppWebPages\Frameset Drop\Drop\SourceCode\Slk\Dll\AppWebPages\Frameset
	xcopy /I Slk\Dll\Properties Drop\Drop\SourceCode\Slk\Dll\Properties
	xcopy /I Slk\slkadm Drop\Drop\SourceCode\Slk\slkadm
	xcopy /I Slk\slkadm\Properties Drop\Drop\SourceCode\Slk\slkadm\Properties
	xcopy /I Slk\Solution Drop\Drop\SourceCode\Slk\Solution
	xcopy /I Src\LearningComponents Drop\Drop\SourceCode\Src\LearningComponents
	xcopy /I Src\LearningComponents\Images Drop\Drop\SourceCode\Src\LearningComponents\Images
	xcopy /I Src\LearningComponents\Properties Drop\Drop\SourceCode\Src\LearningComponents\Properties
	xcopy /I Src\LearningComponents\Resources Drop\Drop\SourceCode\Src\LearningComponents\Resources
	rem -- xcopy /I Src\LearningComponents\UnitTests Drop\Drop\SourceCode\Src\LearningComponents\UnitTests
	rem -- xcopy /I Src\LearningComponents\UnitTests\Properties Drop\Drop\SourceCode\Src\LearningComponents\UnitTests\Properties
	xcopy /I Src\LearningComponents\Utilities Drop\Drop\SourceCode\Src\LearningComponents\Utilities
	xcopy /I Src\Schema Drop\Drop\SourceCode\Src\Schema
	rem -- xcopy /I Src\Schema\Test Drop\Drop\SourceCode\Src\Schema\Test
	xcopy /I Src\SchemaCompiler Drop\Drop\SourceCode\Src\SchemaCompiler
	xcopy /I Src\SchemaCompiler\Properties Drop\Drop\SourceCode\Src\SchemaCompiler\Properties
	rem -- xcopy /I Src\SchemaCompiler\Test Drop\Drop\SourceCode\Src\SchemaCompiler\Test
	rem -- xcopy /I Src\SchemaCompiler\Test\Properties Drop\Drop\SourceCode\Src\SchemaCompiler\Test\Properties
	rem --
	mkdir Drop\Drop\SourceCode\Src\Shared
	copy Src\Shared\dllver.rc Drop\Drop\SourceCode\Src\Shared
	copy Src\Shared\FileVersion.cs Drop\Drop\SourceCode\Src\Shared
	copy Src\Shared\SharedAssemblyInfo.cs Drop\Drop\SourceCode\Src\Shared
	copy Src\Shared\SlkPublicKey.snk Drop\Drop\SourceCode\Src\Shared
	copy Src\Shared\vernum.h Drop\Drop\SourceCode\Src\Shared
	copy Src\Shared\Version.cs Drop\Drop\SourceCode\Src\Shared
	rem --
	xcopy /I Src\SharePoint Drop\Drop\SourceCode\Src\SharePoint
	xcopy /I Src\SharePoint\Properties Drop\Drop\SourceCode\Src\SharePoint\Properties
	rem -- xcopy /I Src\SharePoint\UnitTests Drop\Drop\SourceCode\Src\SharePoint\UnitTests
	rem -- xcopy /I Src\SharePoint\UnitTests\Properties Drop\Drop\SourceCode\Src\SharePoint\UnitTests\Properties
	xcopy /I Src\Storage Drop\Drop\SourceCode\Src\Storage
	xcopy /I Src\Storage\Images Drop\Drop\SourceCode\Src\Storage\Images
	xcopy /I Src\Storage\LearningStore Drop\Drop\SourceCode\Src\Storage\LearningStore
	rem -- xcopy /I Src\Storage\LearningStore\Test Drop\Drop\SourceCode\Src\Storage\LearningStore\Test
	rem -- xcopy /I Src\Storage\LearningStore\Test\Job Drop\Drop\SourceCode\Src\Storage\LearningStore\Test\Job
	rem -- xcopy /I Src\Storage\LearningStore\Test\Misc Drop\Drop\SourceCode\Src\Storage\LearningStore\Test\Misc
	rem -- xcopy /I Src\Storage\LearningStore\Test\Properties Drop\Drop\SourceCode\Src\Storage\LearningStore\Test\Properties
	rem -- xcopy /I Src\Storage\LearningStore\Test\Store Drop\Drop\SourceCode\Src\Storage\LearningStore\Test\Store
	rem -- xcopy /I Src\Storage\LearningStore\Test\TestSchemas Drop\Drop\SourceCode\Src\Storage\LearningStore\Test\TestSchemas
	rem -- xcopy /I Src\Storage\LearningStore\Test\Utility Drop\Drop\SourceCode\Src\Storage\LearningStore\Test\Utility
	xcopy /I Src\Storage\Properties Drop\Drop\SourceCode\Src\Storage\Properties
	rem -- xcopy /I Src\Storage\UnitTests Drop\Drop\SourceCode\Src\Storage\UnitTests
	rem -- xcopy /I Src\Storage\UnitTests\Properties Drop\Drop\SourceCode\Src\Storage\UnitTests\Properties
	rem -- xcopy /I Src\TestUtilities Drop\Drop\SourceCode\Src\TestUtilities
	rem -- xcopy /I Src\TestUtilities\AddLearner Drop\Drop\SourceCode\Src\TestUtilities\AddLearner
	rem -- xcopy /I Src\TestUtilities\AddLearner\Properties Drop\Drop\SourceCode\Src\TestUtilities\AddLearner\Properties
	rem -- xcopy /I Src\TestUtilities\AttemptPackage Drop\Drop\SourceCode\Src\TestUtilities\AttemptPackage
	rem -- xcopy /I Src\TestUtilities\AttemptPackage\Properties Drop\Drop\SourceCode\Src\TestUtilities\AttemptPackage\Properties
	rem -- xcopy /I Src\TestUtilities\ImportPackage Drop\Drop\SourceCode\Src\TestUtilities\ImportPackage
	rem -- xcopy /I Src\TestUtilities\ImportPackage\Properties Drop\Drop\SourceCode\Src\TestUtilities\ImportPackage\Properties
	rem -- xcopy /I Src\TestUtilities\LearningStoreHelpers Drop\Drop\SourceCode\Src\TestUtilities\LearningStoreHelpers
	rem -- xcopy /I Src\TestUtilities\LearningStoreHelpers\Properties Drop\Drop\SourceCode\Src\TestUtilities\LearningStoreHelpers\Properties
	rem -- xcopy /I Src\TestUtilities\Misc Drop\Drop\SourceCode\Src\TestUtilities\Misc
	rem -- xcopy /I Src\TestUtilities\Misc\Properties Drop\Drop\SourceCode\Src\TestUtilities\Misc\Properties

	rem -- Create the ValidatePackage directory...
	mkdir Drop\Drop\ValidatePackage
	copy Samples\ValidatePackage\bin\Release\ValidatePackage.exe Drop\Drop\ValidatePackage
	copy Src\LearningComponents\bin\Release\Microsoft.LearningComponents.dll Drop\Drop\ValidatePackage
	copy ..\SLK.SDK\Release\Microsoft.LearningComponents.Compression.dll Drop\Drop\ValidatePackage

	rem -- Copy files from Doc directory...
	xcopy /I Doc\Root Drop\Drop
	xcopy /I /S Doc\Root Drop\Drop\Install
	xcopy /I /S Doc\Root Drop\Drop\SDK
	xcopy /I /S Doc\Root Drop\Drop\SourceCode
	xcopy /I /S Doc\Root Drop\Drop\ValidatePackage
	xcopy /I /S Doc\Install Drop\Drop\Install
	xcopy /I /S Doc\SourceCode Drop\Drop\SourceCode

	rem -- Create Solitaire.zip files...
	-if exist ..\SLK.Internal mkdir Drop\Drop\Install\Samples
	if exist ..\SLK.Internal ..\SLK.Internal\Tools\DZip.exe Samples\Solitaire Drop\Drop\Install\Samples\Solitaire.zip
	-if exist ..\SLK.Internal mkdir Drop\Drop\SDK\Samples
	if exist ..\SLK.Internal ..\SLK.Internal\Tools\DZip.exe Samples\Solitaire Drop\Drop\SDK\Samples\Solitaire.zip

	rem -- Copy other root-level files...
	copy Src\Schema\InitSchema.sql Drop

	rem -- Copy release PDBs...
	mkdir Drop\ReleasePdb
	copy ..\SLK.SDK\Release\Microsoft.LearningComponents.Compression.pdb Drop\ReleasePdb
	copy Src\LearningComponents\bin\Release\Microsoft.LearningComponents.pdb Drop\ReleasePdb
	copy Src\SharePoint\bin\Release\Microsoft.LearningComponents.SharePoint.pdb Drop\ReleasePdb
	copy Src\Storage\bin\Release\Microsoft.LearningComponents.Storage.pdb Drop\ReleasePdb
	copy Slk\Dll\bin\Release\Microsoft.SharePointLearningKit.pdb Drop\ReleasePdb
	copy Src\SchemaCompiler\bin\Release\SchemaCompiler.pdb Drop\ReleasePdb

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
    $(MAKE) /nologo clean
    cd $(MAKEDIR)

# "cleanall" target: like "clean", but also cleans IDE status files etc.
cleanall: $(SRCDIRS:CMD=cleanall) 
$(SRCDIRS:CMD=cleanall): cleancommon
    @echo.
    @echo -----------------------------------------------------------------
    @echo Cleaning $*
    @echo -----------------------------------------------------------------
    cd $*
    $(MAKE) /nologo cleanall
    cd $(MAKEDIR)

# "cleancommon" target: common to "clean" and "cleanall"
cleancommon: clean_sdkdoc clean_slk clean_drop clean_latestsdk clean_samples

# "clean_sdkdoc" target: clean SDK documentation generated files
clean_sdkdoc:
	-del SdkDoc.csproj.user 2> nul
	-attrib -h -r SdkDoc.suo > nul
	-del SdkDoc.suo 2> nul
	-del SdkDoc.ncb 2> nul
	-del bin\Debug\SdkDoc.dll 2> nul
	-del bin\Debug\SdkDoc.pdb 2> nul
	-rmdir bin\Debug 2> nul
	-rmdir bin 2> nul
	-del obj\Debug\SdkDoc.dll 2> nul
	-del obj\Debug\SdkDoc.pdb 2> nul
	-del obj\SdkDoc.csproj.FileList.txt 2> nul
	-rmdir obj\Debug 2> nul
	-rmdir obj 2> nul
	-rmdir /s /q SdkDoc 2> nul
	-rmdir /s /q TempDoc 2> nul
	-del Src\Schema\SchemaDataStorage2.cs 2> nul

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

