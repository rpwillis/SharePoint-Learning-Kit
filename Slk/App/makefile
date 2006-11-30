#  Copyright (c) Microsoft Corporation. All rights reserved. 

# SharePointLearningKit Web Application makefile

SPDIR=c:\program files\common files\microsoft shared\web server extensions\12
APPDIR=$(SPDIR)\Template\Layouts\SharePointLearningKit

all: deb rel

deb:
	rem -- nothing to do

rel:
	rem -- nothing to do

ide:
	devenv SlkApp.sln

prop:
	cd ..
	$(MAKE) prop
	cd $(MAKEDIR)

# "nmake init" clears Web.config; Visual Studio will update it with current assembly references
init:
	copy /y Web.Config.Base Web.config

clean cleanall:
	-rmdir /s /q Bin 2> nul
	-attrib -h -r SlkApp.suo > nul
	-del SlkApp.suo 2> nul
	-del SlkApp.ncb 2> nul
	-del Web.Config 2> nul

