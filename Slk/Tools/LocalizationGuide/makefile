#  Copyright (c) Microsoft Corporation. All rights reserved. 

# LanguagePack makefile

all: rel

deb:
    BuildAllLanguages.bat debug

rel:
    BuildAllLanguages.bat release

clean cleanall:
	-rmdir /s /q Solution\LangPack 2> nul
	-rmdir /s /q Solution\LanguagePacks 2> null
	-rmdir /s/q bin
	del /f/q solution\*.dll

