#  Copyright (c) Microsoft Corporation. All rights reserved. 

# SharePointLearningKit Web Application makefile

SPDIR=c:\program files\common files\microsoft shared\web server extensions\12
ADMINDIR=$(SPDIR)\Template\Admin\SharePointLearningKit
APPDIR=$(SPDIR)\Template\Layouts\SharePointLearningKit

SAMPLES=AddInstructors.CMD AddToUserWebLists.CMD CreateAssignments.CMD ProvisionFromExcel.CMD ReportPages.CMD SimulateClass.CMD SimulateJobTraining.CMD WebService.CMD

all: deb rel sdk samples

deb: SlkSchema.sql Dll\SlkSchema.cs
	call LinkFrameset Clone
	cd Dll
	$(MAKE) /nologo deb
	cd $(MAKEDIR)
	cd Solution
	$(MAKE) /nologo deb
	cd $(MAKEDIR)
	cd slkadm
	$(MAKE) /nologo deb
	cd $(MAKEDIR)
	cd App
	$(MAKE) /nologo init
	cd $(MAKEDIR)

rel: SlkSchema.sql Dll\SlkSchema.cs
	call LinkFrameset Clone
	cd Dll
	$(MAKE) /nologo rel
	cd $(MAKEDIR)
	cd Solution
	$(MAKE) /nologo rel
	cd $(MAKEDIR)
	cd slkadm
	$(MAKE) /nologo rel
	cd $(MAKEDIR)
	$(MAKE) sdk

sdk:
 	-rmdir /s /q SlkSdk 2> nul
	mkdir SlkSdk
	copy SlkSchema.xml SlkSdk
	copy Dll\SlkSchema.cs SlkSdk
	copy SlkSchema.sql SlkSdk
	copy SlkSettings.xsd SlkSdk
	copy SlkSettings.xsx SlkSdk

samples: nul initsamples $(SAMPLES:CMD=deb)
$(SAMPLES:CMD=deb):
	@echo --
	@echo -- Building Sample: $*
	@echo --
	cd Samples\$*
	$(MAKE) /nologo deb
	cd $(MAKEDIR)

initsamples:
 	-rmdir /s /q Debug 2> nul
	mkdir Debug
	copy Solution\DebugFiles\* Debug

cleansamples: $(SAMPLES:CMD=clean)
 	-rmdir /s /q Debug 2> nul
$(SAMPLES:CMD=clean):
	@echo --
	@echo -- Cleaning Sample: $*
	@echo --
	cd Samples\$*
	$(MAKE) /nologo cleanall
	cd $(MAKEDIR)

SlkSchema.sql SlkUpgrade.sql Dll\SlkSchema.cs : SlkSchema.xml
	..\Src\SchemaCompiler\bin\Debug\SchemaCompiler.exe SlkSchema.xml /OutputInit SlkSchema.sql /OutputUpgrade SlkUpgrade.sql /OutputHelper Dll\SlkSchema.cs /SchemaNamespace Microsoft.SharePointLearningKit.Schema /Namespace Microsoft.SharePointLearningKit

# "nmake prop" copies files that solution installs into SharePoint
prop:
	-mkdir "$(APPDIR)" 2> nul
	-mkdir "$(APPDIR)\Images" 2> nul
	-mkdir "$(APPDIR)\Frameset" 2> nul
	-mkdir "$(APPDIR)\Frameset\Images" 2> nul
	-mkdir "$(APPDIR)\Frameset\Include" 2> nul
	-mkdir "$(APPDIR)\Frameset\Theme" 2> nul
	copy /y Admin\*.aspx "$(ADMINDIR)"
	copy /y Admin\*.gif "$(ADMINDIR)"
	copy /y SlkSchema.sql "$(ADMINDIR)"
	copy /y SlkSettings.xml "$(ADMINDIR)\SlkSettings.xml.dat"
	copy /y App\*.aspx "$(APPDIR)"
	copy /y App\Images\*.gif "$(APPDIR)\Images"
	copy /y App\Include\*.js "$(APPDIR)\Include"
	copy /y App\Include\*.css "$(APPDIR)\Include"
	copy /y App\Frameset\*.aspx "$(APPDIR)\Frameset"
	copy /y App\Frameset\*.htm "$(APPDIR)\Frameset"
	copy /y App\Frameset\Include\*.js "$(APPDIR)\Frameset\Include"
	copy /y App\Frameset\Theme\*.gif "$(APPDIR)\Frameset\Theme"
	copy /y App\Frameset\Theme\*.css "$(APPDIR)\Frameset\Theme"
	copy /y App\Frameset\Images\*.gif "$(APPDIR)\Frameset\Images"
	iisreset

clean cleanall: cleansamples
	call LinkFrameset Clean
	-del SlkSchema.sql 2> nul
	-del SlkUpgrade.sql 2> nul
	-del TempSlkUpgrade.sql 2> nul
	-del Dll\SlkSchema.cs 2> nul
	-del App\localtestrun.testrunconfig
	-del SlkUnitTests\SlkUnitTests.csproj.user 2> nul
 	-rd /s /q SlkSdk 2> nul
	-rd /s /q App\TestResults
	-rd /s /q SlkUnitTests\bin 2> nul
	-rd /s /q SlkUnitTests\obj 2> nul
	cd Dll
	$(MAKE) /nologo cleanall
	cd $(MAKEDIR)
	cd App
	$(MAKE) /nologo cleanall
	cd $(MAKEDIR)
	cd Solution
	$(MAKE) /nologo cleanall
	cd $(MAKEDIR)
	cd slkadm
	$(MAKE) /nologo cleanall
	cd $(MAKEDIR)
	cd App
	$(MAKE) /nologo cleanall
	cd $(MAKEDIR)

